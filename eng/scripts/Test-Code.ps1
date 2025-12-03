#!/usr/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [Parameter(Position=0)]
    [string[]] $Paths,
    [string[]] $Members,
    [ValidateSet('Live', 'Unit', 'All', 'Recorded')]
    [string] $TestType = 'Unit',
    [string] $TestResultsPath,
    [switch] $CollectCoverage,
    [switch] $OpenReport,
    [switch] $TestNativeBuild,
    [switch] $OnlyBuild
)

$ErrorActionPreference = 'Stop'
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$debugLogs = $env:SYSTEM_DEBUG -eq 'true' -or $DebugPreference -eq 'Continue'

$workPath = "$RepoRoot/.work/tests"
Remove-Item -Recurse -Force $workPath -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $workPath -Force | Out-Null

if (!$TestResultsPath) {
    $TestResultsPath = "$workPath/testResults"
}

# Clean previous results
Remove-Item -Recurse -Force $TestResultsPath -ErrorAction SilentlyContinue

# Finds all test projects, then filters them based on the specified path filters.
function FilterTestProjects {
    $fileNameFilters = switch ($testType) {
        'Live' { '*.LiveTests.csproj' }
        'Unit' { '*.UnitTests.csproj' }
        'Recorded' { '*.LiveTests.csproj' }
        'All'  { '*.LiveTests.csproj', '*.UnitTests.csproj' }
    }

    $testProjects = Get-ChildItem -Path "$RepoRoot" -Recurse -Filter "*.csproj" -Include $fileNameFilters -File
    | ForEach-Object { @{
        FullName = $_.FullName
        Relative = (Resolve-Path -Path $_.FullName -Relative -RelativeBasePath $RepoRoot).Replace('\', '/').TrimStart('./')
    }}

    # until all LiveTest projects are migrated to recorded tests, we _must_ complete
    # an additional filter such that we'll only invoke those csprojs where playback is possible
    if ($testType -eq 'Recorded') {
        $testProjects = $testProjects | Where-Object {
            $projectDirectory = Split-Path -Path $_.FullName -Parent
            Test-Path -Path (Join-Path -Path $projectDirectory -ChildPath 'assets.json')
        }
    }

    $normalizedPathFilters = $Paths ? ($Paths | ForEach-Object { "*$($_.Replace('\', '/'))*" }) : @()

    if($normalizedPathFilters) {
        $testProjects = $testProjects | Where-Object {
            foreach($filter in $normalizedPathFilters) {
                if ($_.Relative -like $filter) {
                    return $true
                }
            }
            return $false
        }
    }

    if($testProjects.Count -eq 0) {
        Write-Error "No test projects found for test type '$testType' with the specified filters"
        return $null
    }

    return $testProjects.FullName
}

function CreateTestSolution {
    param(
        [Parameter(Mandatory=$true)]
        [string]$workPath,
        [Parameter(Mandatory=$true)]
        [string[]]$testProjects
    )

    # Create solution and add projects
    Write-Host "Creating temporary solution file..."

    $originalLocation = Get-Location
    Set-Location $workPath
    try {
        dotnet new sln -n "Tests" --format slnx | Out-Null
        dotnet sln add $testProjects --in-root | Out-Host

        $fullPath = Resolve-Path 'Tests.slnx'
        return $fullPath
    }
    finally {
        Set-Location $originalLocation
    }
}

function Create-CoverageReport {
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
# main

$testProjects = FilterTestProjects

$solutionPath = CreateTestSolution -workPath $workPath -testProjects $testProjects

if (!$solutionPath) {
    exit 1
}

Push-Location $workPath
try {
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

    if($OnlyBuild) {
        Write-Host "Just building the test projects, not running tests." -ForegroundColor Yellow
        Invoke-LoggedCommand "dotnet build '$solutionPath' --configuration 'Debug'" -AllowedExitCodes @(0)
        exit $LastExitCode
    }

    $coverageArg = $CollectCoverage ? "--collect:'XPlat Code Coverage'" : ""
    $resultsArg = "--results-directory '$TestResultsPath'"
    $loggerArg = "--logger 'trx' --logger 'console;verbosity=detailed'"

    $command = "dotnet test $coverageArg $resultsArg $loggerArg"

    if($Members.Count -gt 0) {
        $memberFilterString = $Members | ForEach-Object { "FullyQualifiedName~$_" } | Join-String -Separator '|'
        $command += " --filter '$memberFilterString'"
    }

    Invoke-LoggedMsBuildCommand -Command $command -AllowedExitCodes @(0, 1)
}
finally {
    Pop-Location
}

$testExitCode = $LastExitCode

# Coverage Report Generation - only if coverage collection was enabled
if ($CollectCoverage) {
    Create-CoverageReport
}

exit $testExitCode
