#!/bin/env pwsh
#Requires -Version 7

param(
    [string]$Runtime,
    [Parameter(Mandatory=$false)]
    [ValidateSet('Html', 'Console')]
    [string]$OutputFormat = 'Html',
    [string]$ServerName = "Azure.Mcp.Server"
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/AOT-Config.ps1" -ServerName $ServerName
$config = Get-AOTConfig -ServerName $ServerName

$root = $config.RootPath
$projectFile = $config.ProjectFile
$aotReportDir = $config.ReportDirectory
$reportPath = $config.RawReportPath
$jsonReportPath = $config.JsonReportPath

if (-not $Runtime) {
    $runtime = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
} else {
    $runtime = $Runtime
}

function Initialize-AOTAnalysisDirectories {
    param(
        [string]$aotReportDir,
        [string]$projectFile
    )
    
    if (!(Test-Path $aotReportDir)) {
        # Ensure AOT report directory exists
        New-Item -ItemType Directory -Path $aotReportDir -Force | Out-Null
    }
    
    $projectObjDir = Join-Path (Split-Path $projectFile) "obj"
    if (Test-Path $projectObjDir) {
        # Clean project obj directory to ensure fresh build
        Write-Host "Deleting project obj directory: $projectObjDir"
        Remove-Item -Path $projectObjDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}

function Get-DllNameSet {
    param(
        [string]$projectFile
    )
    $objDir = Join-Path (Split-Path $projectFile) "bin"
    $dllNameTable = @{}
    if (Test-Path $objDir) {
        $dlls = Get-ChildItem -Path $objDir -Recurse -Filter *.dll -File
        foreach ($dll in $dlls) {
            $dllName = $dll.BaseName
            $dllNameTable[$dllName] = $true
        }
    }
    # Sort: Returns DLL names sorted with the most specialized (most dots) first.
    return $dllNameTable.Keys |
        Sort-Object @{ Expression = { $_.Split('.').Count }; Descending = $true },
                    @{ Expression = { $_ }; Descending = $true }
}

function Extract-TypeNameFromILWarningLine {
    param(
        [string]$line
    )
    # Match e.g. 'warning IL2026: Azure.Monitor.OpenTelemetry.AspNetCore.DefaultAzureMonitorOptions.Configure(AzureMonitorOptions): ...'
    if ($line -match 'warning IL\d+: ([^:]+):') {
        $full = $Matches[1]
        # Extract type name before last dot and first parenthesis
        if ($full -match '^(.*)\.([^.]+)\(') {
            return $Matches[1]
        } else {
            return $full
        }
    }
    return $null
}

function Find-BestDllMatch {
    param(
        [string]$typeName,
        [string[]]$dllNames # List of DLL names sorted with most specialized first
    )
    
    # Only look for exact matches or prefix matches i.e., typename starts with DLLName
    # This is the most reliable heuristics for .NET assemblies.
    foreach ($dllName in $dllNames) {
        if ($typeName -eq $dllName -or $typeName -like "$dllName.*") {
            return "$dllName.dll"
        }
    }
    # if the heuristics do not find a match, return "unknown"
    return "unknown"
}

function Render-Result {
    param(
        [ValidateSet('Html', 'Console')]
        [string]$OutputFormat = 'Html'
    )
    
    try {
        & "$PSScriptRoot/Render-AOT-Analysis-Result.ps1" -OutputFormat $OutputFormat
        if ($OutputFormat -eq 'Html') {
            Write-Host "HTML report generated: $($config.HtmlReportPath)" -ForegroundColor Green
        }
    } catch {
        Write-Warning "Failed to generate report with format '$OutputFormat': $_"
    }
}

## Main program.

Write-Host "Running AOT compatibility (trimming) analysis for runtime: $runtime ..."

# Initialize directories for AOT analysis
Initialize-AOTAnalysisDirectories -aotReportDir $aotReportDir -projectFile $projectFile

$publishArgs = @(
    'publish', $projectFile,
    '--configuration', 'Release',  # Always use Release for AOT analysis
    '--runtime', $runtime,
    '--self-contained', 'true',
    '/p:PublishTrimmed=true',
    '/p:TrimmerSingleWarn=false',
    '/p:TreatWarningsAsErrors=false'  # Disable treating warnings as errors so AOT warnings do not fail the analysis
)

Write-Host "Executing: dotnet $($publishArgs -join ' ')"

$output = & dotnet @publishArgs 2>&1
$exitCode = $LASTEXITCODE

# Save raw output regardless of success/failure
$output | Out-File -FilePath $reportPath -Encoding utf8

# Check if dotnet publish failed
if ($exitCode -ne 0) {
    Write-Host "dotnet publish command for AOT analysis failed with exit code: $exitCode" -ForegroundColor Red
    Write-Host "See $reportPath for detailed error information." -ForegroundColor Red
    # Write empty JSON object for consistency
    @{} | ConvertTo-Json | Out-File -FilePath $jsonReportPath -Encoding utf8
    exit $exitCode
}


$dllNameSet = Get-DllNameSet -projectFile $projectFile
$warnings = $output | Select-String 'warning IL'

if ($warnings.Count -gt 0) {
    Write-Host "AOT compatibility analysis complete. See $reportPath for detailed AOT raw warnings."
    
    $reportObject = @{}
    $dllMatchCache = @{}
    
    foreach ($w in $warnings) {
        $line = $w.Line
        $typeName = Extract-TypeNameFromILWarningLine -line $line
        if ($typeName) {
            $dllName = $null
            if ($dllMatchCache.ContainsKey($typeName)) {
                $dllName = $dllMatchCache[$typeName]
            } else {
                $dllName = Find-BestDllMatch -typeName $typeName -dllNames $dllNameSet
                $dllMatchCache[$typeName] = $dllName
            }
            
            if (-not $reportObject.ContainsKey($dllName)) {
                $reportObject[$dllName] = @()
            }
            $reportObject[$dllName] += $line
        }
    }
    
    # Write JSON report to file
    $reportObject | ConvertTo-Json -Depth 3 | Out-File -FilePath $jsonReportPath -Encoding utf8
    Write-Host "See $jsonReportPath for AOT warnings in JSON format." -ForegroundColor Green
    Render-Result -OutputFormat $OutputFormat
    exit 1
} else {
    Write-Host "AOT compatibility analysis complete. No trimmer/AOT warnings found. See $reportPath."
    @{} | ConvertTo-Json | Out-File -FilePath $jsonReportPath -Encoding utf8
    Write-Host "Empty JSON report written to: $jsonReportPath" -ForegroundColor Green
    Render-Result -OutputFormat $OutputFormat
    exit 0
}
