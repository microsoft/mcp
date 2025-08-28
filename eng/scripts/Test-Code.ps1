#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $TestResultsPath,
    [string[]] $Areas,  # for PR
    [ValidateSet('Live', 'Unit', 'All')]
    [string] $TestType = 'Unit',
    [switch] $CollectCoverage,
    [switch] $OpenReport,
    [switch] $TestNativeBuild,
    [string] $ServerName
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

Write-Host "Input Areas: $($Areas -join ', ')" -ForegroundColor Cyan

$debugLogs = $env:SYSTEM_DEBUG -eq 'true' -or $DebugPreference -eq 'Continue'

$workPath = "$RepoRoot/.work/tests"
Remove-Item -Recurse -Force $workPath -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $workPath -Force | Out-Null

if (!$TestResultsPath) {
    $TestResultsPath = "$workPath/testResults"
}

# Clean previous results
Remove-Item -Recurse -Force $TestResultsPath -ErrorAction SilentlyContinue

# Groups areas by their server based on naming convention.
function groupAreasByServer {
    param(
        [Parameter(Mandatory=$true)]
        [string[]]$areas
    )

    $allServers = @(Get-ChildItem -Path "$RepoRoot/servers" -Directory | ForEach-Object { $_.Name })
    Write-Host "Available servers: $($allServers -join ', ')" -ForegroundColor Cyan

    $servers = @() # e.g. (Azure.Mcp., Fabric.Mcp., Template.Mcp.)
    foreach ($server in $allServers) {
        if ($server -match '^(.+)\.Server$') {
            $servers += "$($matches[1])."
        } else {
            Write-Warning "Server '$server' doesn't match expected naming pattern '*.Server'"
        }
    }

    $serverAreasMap = @{}
    foreach ($area in $areas) {
        if ($area -match '^(tools|core|servers)/([^/]+)') {
            $areaProjName = $matches[2] # e.g. Azure.Mcp.Core, Azure.Mcp.Tools.Acr, Fabric.Mcp.Tools.EventStream

            # Find the server that the area belongs to.
            $serverName = $null
            
            foreach ($server in $servers) {
                if ($areaProjName.StartsWith($server)) {
                    $serverName = "$($server.TrimEnd('.')).Server"
                    break
                }
            }
            
            if (-not $serverName) {
                Write-Warning "No matching server found for the area '$area'. Available mcp server types: $($servers -join ', ')"
                continue
            }
            if (-not $serverAreasMap.ContainsKey($serverName)) {
                $serverAreasMap[$serverName] = @()
            }
            $serverAreasMap[$serverName] += $area
        }
    }
    return $serverAreasMap
}

# Gets all area projects those are excluded using BuildNative condition.
function Get-NativeExcludedAreas {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ServerName
    )
    
    $areaPathPattern = '(tools[/\\][^/\\]+)[/\\]src'
    $ProjectFile = "$RepoRoot/servers/$ServerName/src/$ServerName.csproj"

    if (!(Test-Path $ProjectFile)) {
        Write-Error "$ProjectFile not found"
        exit 1
    }

    [xml]$xml = Get-Content $ProjectFile
    $buildNativeGroup = $xml.Project.ItemGroup | Where-Object { $_.Condition -eq "'`$(BuildNative)' == 'true'" }

    if (!$buildNativeGroup) {
        Write-Warning "No ItemGroup with BuildNative condition found"
        return @()
    }

    $excludedAreas = @()
    foreach ($ref in $buildNativeGroup.ProjectReference) {
        if ($ref.Remove -match $areaPathPattern) {
            $excludedAreas += $matches[1].Replace('\', '/')
        }
    }

    Write-Host "Areas excluded in $ServerName.csproj: $($excludedAreas -join ', ')"
    return $excludedAreas
}

# Analyzes areas for native compatibility and returns servers, native-compatible areas, and incompatible areas.
function Get-NativeCompatibleAreasAndServers {
    param(
        [Parameter(Mandatory=$true)]
        [string[]]$areas
    )

    $serverAreasMap = groupAreasByServer -areas $areas

    $servers = @($serverAreasMap.Keys)
    $nonNativeAreas = @()
    $nativeAreas = @()
    
    foreach ($server in $servers) {
        $serverAreas = $serverAreasMap[$server]
        $excludedAreas = Get-NativeExcludedAreas -ServerName $server
        $nonNativeAreas += @($serverAreas | Where-Object { $_ -in $excludedAreas })
        $nativeAreas += @($serverAreas | Where-Object { $_ -notin $excludedAreas })
    }

    return @{
        Servers = $servers
        NativeAreas = $nativeAreas
        NonNativeAreas = $nonNativeAreas
    }
}


# Identifies the root directories to be recursively scanned for tests in the specified areas.
function GetTestsRootDirs {
    param(
        [string[]]$areas
    )

    $testsRootDirs = @()

    if (!$areas) {
        return $testsRootDirs += $RepoRoot
    }

    foreach ($area in $areas) {
        $rootedPath = "$RepoRoot/$area"
        if (Test-Path $rootedPath) {
            $testsRootDirs += $rootedPath
        } else {
            Write-Error "Area path '$rootedPath' does not exist."
            return $null
        }
    }

    return $testsRootDirs
}

function BuildNativeServersAndPrepareTests {
    param(
        [Parameter(Mandatory=$true)]
        [string[]]$testsRootDirs,
        [Parameter(Mandatory=$true)]
        [string[]]$servers
    )

    $nativeBinaryPaths = @()
    foreach ($server in $servers) {
        # Native AOT compilation only occurs during 'dotnet publish', not 'dotnet build'
        $nativeBinaryPath = PublishNativeBinary -ServerName $server
        $nativeBinaryPaths += $nativeBinaryPath
    }

    Write-Host "Building test project(s)"
    Invoke-LoggedCommand `
        -Command "dotnet build" `
        -AllowedExitCodes @(0)
    
    foreach ($nativeBinaryPath in $nativeBinaryPaths) {
        CopyNativeBinaryToTestDirs -nativeBinaryPath $nativeBinaryPath -testsRootDirs $testsRootDirs
    }
}

function PublishNativeBinary {
    param(
        [Parameter(Mandatory=$true)]
        [string]$ServerName
    )
    
    $runtimeId = [System.Runtime.InteropServices.RuntimeInformation]::RuntimeIdentifier
    Write-Host "Publishing $ServerName as native binary for $runtimeId"

    $projectDir = "$RepoRoot/servers/$ServerName/src"

    Invoke-LoggedCommand `
        -Command "dotnet publish '$projectDir/$ServerName.csproj' -c Release -r $runtimeId /p:BuildNative=true" `
        -AllowedExitCodes @(0) | Out-Null

    $exeName = if ($runtimeId.StartsWith('win-')) { "azmcp.exe" } else { "azmcp" }
    $nativeExePath = "$projectDir/bin/Release/net9.0/$runtimeId/publish/$exeName"

    if (-not (Test-Path $nativeExePath)) {
        Write-Error "Native binary not found at $nativeExePath"
        exit 1
    }

    return $nativeExePath
}

function CopyNativeBinaryToTestDirs {
    param(
        [Parameter(Mandatory=$true)]
        [string]$nativeBinaryPath,
        [string[]]$testsRootDirs
    )
    Write-Host "Copying native $nativeBinaryPath to test directories"

    $testsRootDirs | ForEach-Object {
        Get-ChildItem -Path $_ -Recurse -Filter "*.LiveTests" -Directory
    } | ForEach-Object {
        $targetDirectory = "$($_.FullName)/bin/Debug/net9.0"
        Copy-Item $nativeBinaryPath $targetDirectory -Force
    }
}

function CreateTestSolution {
    param(
        [Parameter(Mandatory=$true)]
        [string]$workPath,
        [Parameter(Mandatory=$true)]
        [string[]]$testsRootDirs,
        [Parameter(Mandatory=$true)]
        [string]$testType
    )

    $testPatterns = switch ($testType) {
        'Live' { @('*.LiveTests.csproj') }
        'Unit' { @('*.UnitTests.csproj') }
        'All'  { @('*.LiveTests.csproj', '*.UnitTests.csproj') }
        default {
            Write-Error "Invalid test type specified: '$testType'. Valid options are 'Live', 'Unit', or 'All'."
            return $null
        }
    }

    $testProjects = @($testsRootDirs | ForEach-Object {
        $testsRootDir = $_
        $testPatterns | ForEach-Object {
            Get-ChildItem $testsRootDir -Recurse -File -Filter $_
        }
    })

    if($testProjects.Count -eq 0) {
        Write-Error "No test projects found in the specified areas for test type '$testType'."
        return $null
    }

    # Create solution and add projects
    Write-Host "Creating temporary solution file..."

    Push-Location $workPath
    try {
        dotnet new sln -n "Tests" | Out-Null
        dotnet sln add $testProjects --in-root | Out-Host
    }
    finally {
        Pop-Location
    }

    return "$workPath/Tests.sln"
}

# main

$areas = $Areas

if ($TestNativeBuild) {
    if (-not $areas -or $areas.Count -eq 0) {
        Write-Error "Areas parameter is required and must contain at least one area when TestNativeBuild is specified."
        exit 1
    }

    $result = Get-NativeCompatibleAreasAndServers -areas $areas

    if ($result.Servers.Count -eq 0) {
        Write-Warning "No area to mcp server mapping exists for $($areas -join ', ')"
        exit 0
    }

    $servers = $result.Servers
    $areas = $result.NativeAreas
    $nonNativeAreas = $result.NonNativeAreas

    if ($areas.Count -eq 0) {
        Write-Warning "All the specified area(s) [$($nonNativeAreas -join ', ')] are native incompatible, specify areas that support native builds or run without -TestNativeBuild."
        exit 0
    }

    if ($nonNativeAreas.Count -gt 0) {
        Write-Warning "The following native incompatible areas will be excluded from native tests:"
        Write-Warning "  $($nonNativeAreas -join ', ')"
    }
}

$testsRootDirs = GetTestsRootDirs -areas $areas

if (!$testsRootDirs) {
    exit 1
}

$solutionPath = CreateTestSolution -workPath $workPath -testsRootDirs $testsRootDirs -testType $TestType

if (!$solutionPath) {
    exit 1
}

Push-Location $workPath
try {
    if ($TestNativeBuild) {
        BuildNativeServersAndPrepareTests -testsRootDirs $testsRootDirs -servers $servers
    }

    if($debugLogs) {
        Write-Host "`n`n"
        # dump all environment variables
        Write-Host "Current environment variables:" -ForegroundColor Yellow
        Get-ChildItem Env: | Sort-Object Name | ForEach-Object { "$($_.Name)= $($_.Value)" } | Out-Host

        # dump az powershell context
        Write-Host "`nCurrent Azure PowerShell context (Get-AzContext):" -ForegroundColor Yellow
        try {
            Get-AzContext | ConvertTo-Json | Out-Host
        } catch {
            Write-Host "Error retrieving Azure PowerShell context: $($_.Exception.Message)" -ForegroundColor Red
        }

        # dump az cli context
        Write-Host "`nCurrent Azure CLI context (az account show):" -ForegroundColor Yellow
        try {
            az account show | ConvertTo-Json | Out-Host
        } catch {
            Write-Host "Error retrieving Azure CLI context: $($_.Exception.Message)" -ForegroundColor Red
        }
        Write-Host "`n`n"
    }

    $coverageArg = $CollectCoverage ? "--collect:'XPlat Code Coverage'" : ""
    $resultsArg = "--results-directory '$TestResultsPath'"
    $loggerArg = "--logger 'trx'"

    $command = "dotnet test $coverageArg $resultsArg $loggerArg"
    if ($TestNativeBuild) {
        $command += " --no-build"
    }

    Invoke-LoggedCommand `
        -Command $command `
        -AllowedExitCodes @(0, 1)
}
finally {
    Pop-Location
}

$testExitCode = $LastExitCode

# Coverage Report Generation - only if coverage collection was enabled
if ($CollectCoverage) {
    # Find the coverage file
    $coverageFile = Get-ChildItem -Path $TestResultsPath -Recurse -Filter "coverage.cobertura.xml"
    | Where-Object { $_.FullName.Replace('\','/') -notlike "*/in/*" }
    | Select-Object -First 1

    if (-not $coverageFile) {
        Write-Error "No coverage file found!"
        exit 1
    }

    if ($env:TF_BUILD) {
        # Write the path to the cover file to a pipeline variable
        Write-Host "##vso[task.setvariable variable=CoverageFile]$($coverageFile.FullName)"
    } else {
        # Ensure reportgenerator tool is installed
        if (-not (Get-Command reportgenerator -ErrorAction SilentlyContinue)) {
            Write-Host "Installing reportgenerator tool..."
            dotnet tool install -g dotnet-reportgenerator-globaltool
        }

        # Generate reports
        Write-Host "Generating coverage reports..."

        $reportDirectory = "$TestResultsPath/coverageReport"
        Invoke-LoggedCommand ("reportgenerator" +
        " -reports:'$coverageFile'" +
        " -targetdir:'$reportDirectory'" +
        " -reporttypes:'Html;HtmlSummary;Cobertura'" +
        " -assemblyfilters:'+azmcp'" +
        " -classfilters:'-*Tests*;-*Program'" +
        " -filefilters:'-*JsonSourceGenerator*;-*LibraryImportGenerator*'")

        Write-Host "Coverage report generated at $reportDirectory/index.html"

        # Open the report in default browser
        $reportPath = "$reportDirectory/index.html"
        if (-not (Test-Path $reportPath)) {
            Write-Error "Could not find coverage report at $reportPath"
            exit 1
        }

        if ($OpenReport) {
            # Open the report in default browser
            Write-Host "Opening coverage report in browser..."
            if ($IsMacOS) {
                # On macOS, use 'open' command
                Start-Process "open" -ArgumentList $reportPath
            } elseif ($IsLinux) {
                # On Linux, use 'xdg-open'
                Start-Process "xdg-open" -ArgumentList $reportPath
            } else {
                # On Windows, use 'Start-Process'
                Start-Process $reportPath
            }
        }
    }

    # Command Coverage Summary
    try{
        $CommandCoverageSummaryFile = "$TestResultsPath/Coverage.md"

        $xml = [xml](Get-Content $coverageFile.FullName)

        $classes = $xml.coverage.packages.package.classes.class |
            Where-Object { $_.name -match 'AzureMcp\.(.*\.)?Commands\.' -and $_.filename -notlike '*System.Text.Json.SourceGeneration*' }

        $fileGroups = $classes |
            Group-Object { $_.filename } |
            Sort-Object Name

        $summary = $fileGroups | ForEach-Object {
            # for live tests, we only want to look at the ExecuteAsync methods
            $methods = if($Live) {
                $_.Group | ForEach-Object {
                    if($_.name -like '*<ExecuteAsync>*'){
                        # Generated code for async ExecuteAsync methods
                        return $_.methods.method
                    } else {
                        # Non async methods named ExecuteAsync
                        return $_.methods.method | Where-Object { $_.name -eq 'ExecuteAsync' }
                    }
                }
            }
            else {
                $_.Group.methods.method
            }

            $lines = $methods.lines.line
            $covered = ($lines | Where-Object { $_.hits -gt 0 }).Count
            $total = $lines.Count

            if($total) {
                return [pscustomobject]@{
                    file = $_.name
                    pct = if ($total -gt 0) { $covered * 100 / $total } else { 0 }
                    covered = $covered
                    lines = $total
                }
            }
        }

        $maxFileWidth = ($summary | Measure-Object { $_.file.Length } -Maximum).Maximum
        if ($maxFileWidth -le 0) {
            $maxFileWidth = 10
        }
        $header = $live ? "Live test code coverage for command ExecuteAsync methods" : "Unit test code coverage for command classes"

        $output = ($env:TF_BUILD ? "" : "$header`n`n") +
                "File $(' ' * ($maxFileWidth - 5)) | % Covered | Lines | Covered`n" +
                "$('-' * $maxFileWidth) | --------: | ----: | ------:`n"

        $summary | ForEach-Object {
            # Format each line with the appropriate width
            $output += ("{0,-$maxFileWidth} | {1,9:F0} | {2,5} | {3,7}`n" -f $_.file, $_.pct, $_.lines, $_.covered)
        }

        Write-Host "Writing command coverage summary to $CommandCoverageSummaryFile"
        $output | Out-File -FilePath $CommandCoverageSummaryFile -Encoding utf8

        if ($env:TF_BUILD) {
            Write-Host "##vso[task.addattachment type=Distributedtask.Core.Summary;name=$header;]$(Resolve-Path $CommandCoverageSummaryFile)"
        }
    }
    catch {
        Write-Host "Error creating coverage summary: $($_.Exception.Message)"
        Write-Host "Stack trace: $($_.Exception.StackTrace)"
        exit 1
    }
}

exit $testExitCode
