#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [switch] $SetDevOpsVariables,
    [ValidateSet('Live', 'Unit', 'All')]
    [string] $TestType = 'All',
    [string] $ServerName
)

. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# When "core" is modified, include storage and keyVault as the canary service tools.
$canaryPaths = @{
    "core/Azure.Mcp.Core"= @('tools/Azure.Mcp.Tools.Storage', 'tools/Azure.Mcp.Tools.KeyVault')
    "core/Microsoft.Mcp.Core"= @('tools/Azure.Mcp.Tools.Storage', 'tools/Azure.Mcp.Tools.KeyVault')
}

# While there is a "core" directory at the repo root, we consider the "core" path to be all of the repo outside of the
# "tools" directory.
# This lets us make simple statements like:
# - Changes in eng/ are "core" changes
# - Changes in core/ are "core" changes
# - Changes to tools/Azure.Mcp.Tools.Redis are "Azure.Mcp.Tools.Redis" changes
# - If you change any "core" files, we need to test all of the "core" path as well as a few canary tools
# - If you change just tool files, we need to test the tools you changed

# If the caller passed in a ServiceName, then only the tools that the server depends on are in scope to test
# Otherwise, all tools in the tools/ directory are in scope

$paths = if ($ServerName) {
    Write-Host "Getting list of project references for $serverName"
    $serverProject = "$RepoRoot/servers/$ServerName/src/$ServerName.csproj"
    dotnet build $serverProject /t:ListProjectReferences  /p:OutputFile="$RepoRoot/.work/refs.json" | Out-Null
    $projectReferences = Get-Content "$RepoRoot/.work/refs.json" | ConvertFrom-Json

    @($serverProject, $projectReferences)
} else {
    Get-ChildItem $RepoRoot -Directory -Recurse
}

# Reduce down to paths like:
#   tools/Azure.Mcp.Tools.Storage
#   core/Fabric.Mcp.Core
#   servers/Azure.Mcp.Server

$paths = $paths
    | Resolve-Path -Relative -RelativeBasePath $RepoRoot
    | Where-Object { ($_.Replace('\', '/') -replace '^./', '') -match '^((tools|servers|core)/[^/]+)/' }
    | ForEach-Object { $Matches[1] }
    | Sort-Object -Unique

Push-Location $RepoRoot
try {
    $isPullRequestBuild = $env:BUILD_REASON -eq 'PullRequest'

    if($isPullRequestBuild) {
        # If we're in a pull request, use the set of changed files to narrow down the set of paths to test.
        $changedFiles = Get-ChangedFiles
        # $changedFiles = [
        #   tools/Azure.Mcp.Tools.Storage/src/someFile.cs    <- "Azure.Mcp.Tools.Storage"
        #   tools/Azure.Mcp.Tools.Monitoring/README.md       <- "Azure.Mcp.Tools.Monitoring"
        #   core/src/commonClass.cs                          <- "Core"
        #   eng/scripts/SomeScript.ps1                       <- "Core"
        # ]
        Write-Host ''

        # Currently, we don't exclude non-code files from the changed files list.
        # For example, updating a markdown file in a service path will still trigger tests for that path.
        # Updating a file outside of the defined paths will be seen as a change to the core path.
        $changedPaths = @($changedFiles
        | ForEach-Object { $_ -match '^((tools|core|servers)/.*?)/' -and $paths -contains $Matches[1] ? $Matches[1] : 'core/Microsoft.Mcp.Core' }
        | Sort-Object -Unique)

        <# Making $changedPaths = @(
            'tools/Azure.Mcp.Tools.Storage',
            'tools/Azure.Mcp.Tools.Monitoring',
            'core/Microsoft.Mcp.Core'
        ) #>

        if($changedPaths.Count -eq 0) {
            Write-Host "No changed, testable paths detected. Defaulting to core." -ForegroundColor Yellow
            $changedPaths = @('core/Microsoft.Mcp.Core')
        } else {
            Write-Host "Changed paths detected: $($changedPaths -join ', ')"
        }

        $pathsToTest = $changedPaths
        # If any affected path has "canaries", add them to the paths to test
        foreach ($canaryKey in $canaryPaths.Keys) {
            if($changedPaths -contains $canaryKey) {
                $canaries = $canaryPaths[$canaryKey]
                Write-Host "$canaryKey changes detected. Including canary paths: $($canaries -join ', ')" -ForegroundColor Cyan
                $pathsToTest += $canaries
            }
        }

        $pathsToTest = @($pathsToTest | Sort-Object -Unique)

        <# Making $pathsToTest = @(
            'tools/Azure.Mcp.Tools.Storage',
            'tools/Azure.Mcp.Tools.Monitoring',
            'core/Microsoft.Mcp.Core',
            'tools/Azure.Mcp.Tools.KeyVault'  <-- from Microsoft.Mcp.Core's canary list
        ) #>
    } else {
        # If we're not in a pull request, test all paths
        $pathsToTest = $paths
    }

    Write-Host "Forming test matrix"
    $testMatrix = [ordered]@{}
    foreach ($path in $pathsToTest) {
        $testResourcesPath = "$path/tests"
        $hasTestResources = Test-Path "$RepoRoot/$testResourcesPath/test-resources.bicep"
        $hasLiveTests = (Get-ChildItem $testResourcesPath -Filter '*.LiveTests.csproj' -Recurse).Count -gt 0
        $hasUnitTests = (Get-ChildItem $testResourcesPath -Filter '*.UnitTests.csproj' -Recurse).Count -gt 0

        if ($TestType -eq 'Live' -and (!$hasLiveTests -or !$hasTestResources)) {
            Write-Host "$path has changes, but no live tests or test resources found. Skipping." -ForegroundColor Yellow
            continue
        }

        if ($TestType -eq 'Unit' -and !$hasUnitTests) {
            Write-Host "$path has changes, but no unit tests found. Skipping." -ForegroundColor Yellow
            continue
        }

        $testMatrix[$path] = [ordered]@{
            # We can't use the name 'Path' here because it would override the Path environment variable in matrix based jobs
            PathToTest = $path
            UnitTests = $hasUnitTests
            LiveTests = $hasLiveTests
            TestResources = $hasTestResources
            TestResourcesPath = $testResourcesPath
        }
    }

    $hasTestPaths = $testMatrix.Count -gt 0

    if($SetDevOpsVariables) {
        # Set DevOps variables for changed paths
        $json = ConvertTo-Json $testMatrix -Compress
        Write-Host "##vso[task.setvariable variable=TestMatrix;isOutput=true]$json"
        # Set a variable indicating if any paths changed
        Write-Host "##vso[task.setvariable variable=HasTestPaths;isOutput=true]$hasTestPaths"
    }

    Write-Host ""
    Write-Host "TestMatrix:"
    $testMatrix | ConvertTo-Json | Out-Host

    Write-Host "HasTestPaths: $hasTestPaths"
}
finally {
    Pop-Location
}
