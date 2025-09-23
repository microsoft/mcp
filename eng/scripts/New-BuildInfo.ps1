#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [parameter(Mandatory)]
    [ValidateSet('none', 'internal', 'public')]
    [string] $PublishTarget,
    [parameter(Mandatory)]
    [int] $BuildId,
    [string] $OutputPath,
    [string] $ServerName,
    [switch] $IncludeNative,
    [switch] $TestPipeline
)

. "$PSScriptRoot/../common/scripts/common.ps1"
. "$PSScriptRoot/helpers/PathHelpers.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$inPipelineRun = $env:TF_BUILD -eq "True"

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build_info.json"
}

$serverDirectories = Get-ChildItem "$RepoRoot/servers" -Directory
$toolDirectories = Get-ChildItem "$RepoRoot/tools" -Directory
$coreDirectories = Get-ChildItem "$RepoRoot/core" -Directory

# Public releases always use the version from the repo without a dynamic prerelease suffix, except for test pipelines
# which always use a dynamic prerelease suffix to allow for multiple releases from the same commit
$dynamicPrereleaseVersion = $PublishTarget -ne 'public' -or $TestPipeline

function Get-PathsToTest {
    # When "core" is modified, include storage and keyVault as the canary service tools.
    # TODO: These should be sources from csproj files
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
        if (-not (Test-Path $serverProject)) {
            Write-Error "No project for $ServerName found at $serverProject"
            exit 1
        }

        $projectReferences = (dotnet build $serverProject -getItem:ProjectReference | ConvertFrom-Json).Items.ProjectReference.FullPath

        # We can put full paths here because they'll be reduced to relative project directory paths in the "reduce down" step below
        @($serverProject, $projectReferences)
    } else {
        @($coreDirectories + $serverDirectories + $toolDirectories)
    }

    # Reduce down to paths like:
    #   tools/Azure.Mcp.Tools.Storage
    #   core/Fabric.Mcp.Core
    #   servers/Azure.Mcp.Server

    $projectDirectoryPattern = '^(tools|servers|core)/[^/]+'

    $normalizedPaths = $paths
        | Get-RepoRelativePath -NormalizeSeparators
        | Where-Object { $_ -match $projectDirectoryPattern }
        | ForEach-Object { $Matches[0] }
        | Sort-Object -Unique

    $isPullRequestBuild = $env:BUILD_REASON -eq 'PullRequest'

    if($isPullRequestBuild) {
        # If we're in a pull request, use the set of changed files to narrow down the set of paths to test.
        $changedFiles = Get-ChangedFiles
        # Assuming $changedFiles = [
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
        | ForEach-Object { $_ -match $projectDirectoryPattern -and $normalizedPaths -contains $Matches[1] ? $Matches[1] : 'core/Microsoft.Mcp.Core' }
        | Sort-Object -Unique)

        <# This makes $changedPaths = @(
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

        $normalizedPaths = @($pathsToTest | Sort-Object -Unique)

        <# Making $paths = @(
            'tools/Azure.Mcp.Tools.Storage',
            'tools/Azure.Mcp.Tools.Monitoring',
            'core/Microsoft.Mcp.Core',
            'tools/Azure.Mcp.Tools.KeyVault'  <-- from Microsoft.Mcp.Core's canary list
        ) #>
    }

    $pathsToTest = @()
    foreach ($path in $normalizedPaths) {
        $testResourcesPath = "$path/tests"
        $rootedTestResourcesPath = "$RepoRoot/$testResourcesPath"
        $hasTestResources = Test-Path "$rootedTestResourcesPath/test-resources.bicep"
        $hasLiveTests = (Get-ChildItem $rootedTestResourcesPath -Filter '*.LiveTests.csproj' -Recurse).Count -gt 0
        $hasUnitTests = (Get-ChildItem $rootedTestResourcesPath -Filter '*.UnitTests.csproj' -Recurse).Count -gt 0

        $pathsToTest += @{
            path = $path
            hasTestResources = $hasTestResources
            testResourcesPath = $hasTestResources ? $testResourcesPath : $null
            hasLiveTests = $hasLiveTests
            hasUnitTests = $hasUnitTests
        }
    }

    return $pathsToTest
}

function Get-TestMatrix {
    param(
        [hashtable[]] $pathsToTest,
        [ValidateSet('Unit', 'Live')]
        [string] $TestType
    )

    Write-Host "Forming $($TestType.ToLower()) test matrix"
    $testMatrix = [ordered]@{}
    foreach ($path in $pathsToTest) {
        $entry = [ordered]@{
            # We can't use the name 'Path' here because it would override the Path environment variable in matrix based jobs
            pathToTest = $path.Path
        }


        if ($TestType -eq 'Live') {
            if (!$path.HasLiveTests -or !$path.HasTestResources) {
                continue
            }

            $entry.testResourcesPath = $path.TestResourcesPath
            $entry.hasTestResources = $path.HasTestResources
        }

        if ($TestType -eq 'Unit' -and !$path.HasUnitTests) {
            continue
        }

        $testMatrix[$path.Path] = $entry
    }

    return $testMatrix
}

function Get-ServerDetails {
    $serverProjects = $serverDirectories | Get-ChildItem -Filter "src/*.csproj"

    if ($ServerName) {
        $serverProjects = $serverProjects | Where-Object { $_.Directory.Name -ieq $ServerName }
        if ($serverProjects.Count -eq 0) {
            Write-Error "No server directory found with name $ServerName in $RepoRoot/servers."
            exit 1
        }
    }

    $architectures = @('x64', 'arm64')
    $operatingSystems = @(
        @{ name = 'linux'; nodeName = 'linux'; dotnetName = 'linux'; extension = '' }
        @{ name = 'macos'; nodeName = 'darwin'; dotnetName = 'osx'; extension = '' }
        @{ name = 'windows'; nodeName = 'win32'; dotnetName = 'win'; extension = '.exe' }
    )

    $serverProperties = @()

    foreach($serverProject in $serverProjects) {
        $props = & "$PSScriptRoot/Get-ProjectProperties.ps1" -Path $serverProject

        $serverName = $serverProject.BaseName
        $version = [AzureEngSemanticVersion]::new($props.Version)

        if ($dynamicPrereleaseVersion) {
            $version.PrereleaseLabel = 'alpha'
            $version.PrereleaseNumber = $BuildId
        }

        $platforms = @()
        foreach ($os in $operatingSystems) {
            foreach ($arch in $architectures) {
                $name = "$($os.name)-$arch"

                $platforms += [ordered]@{
                    name = $name
                    artifactPath = "$serverName/$name"
                    operatingSystem = $os.name
                    nodeOs = $os.nodeName
                    dotnetOs = $os.dotnetName
                    architecture = $arch
                    extension = $os.extension
                    native = $false
                }

                if($IncludeNative -and $props.IsAotCompatible -eq 'true') {
                    $name = "$($os.name)-$arch-native"
                    $platforms += [ordered]@{
                        name = $name
                        artifactPath = "$serverName/$name"
                        operatingSystem = $os.name
                        nodeOs = $os.nodeName
                        dotnetOs = $os.dotnetName
                        architecture = $arch
                        extension = $os.extension
                        native = $true
                    }
                }
            }
        }

        $serverProperties += [ordered]@{
            name = $serverProject.BaseName
            path = Get-RepoRelativePath -Path $serverProject -NormalizeSeparators
            artifactPath = "$serverName"
            version = $version.ToString()
            cliName = $props.CliName
            assemblyTitle = $props.AssemblyTitle
            description = $props.Description
            readmeUrl = $props.ReadmeUrl
            readmePath = $props.ReadmePath | Get-RepoRelativePath -NormalizeSeparators
            npmPackageName = $props.NpmPackageName
            npmDescription = $props.NpmDescription
            npmPackageKeywords = @($props.NpmPackageKeywords -split ', *')
            dockerImageName = $props.DockerImageName
            dockerDescription = $props.DockerDescription
            dnxPackageId = $props.DnxPackageId
            dnxDescription = $props.DnxDescription
            dnxToolCommandName = $props.DnxToolCommandName
            vsixPackageId = $props.VsixPackageId
            vsixDescription = $props.VsixDescription
            platforms = $platforms
        }
    }

    return $serverProperties
}

Push-Location $RepoRoot
try {
    $serverDetails = Get-ServerDetails
    $pathsToTest = Get-PathsToTest
    $unitTestMatrix = Get-TestMatrix $pathsToTest -TestType 'Unit'
    $liveTestMatrix = Get-TestMatrix $pathsToTest -TestType 'Live'

    $buildInfo = [ordered]@{
        buildId = $BuildId
        publishTarget = $PublishTarget
        dynamicPrereleaseVersion = $dynamicPrereleaseVersion
        servers = $serverDetails
        pathsToTest = $pathsToTest
        unitTestMatrix = $unitTestMatrix
        liveTestMatrix = $liveTestMatrix
    }

    Write-Host "Writing build info to $OutputPath"
    $parentDirectory = Split-Path $OutputPath -Parent
    New-Item -Path $parentDirectory -ItemType Directory -Force | Out-Null

    $buildInfo | ConvertTo-Json -Depth 5 | Out-File -FilePath $OutputPath -Encoding utf8 -Force

    if($inPipelineRun) {
        # Set DevOps variables for test matrices
        Write-Host "##vso[task.setvariable variable=LiveTestMatrix;isOutput=true]$(ConvertTo-Json $liveTestMatrix -Compress)"
        Write-Host "##vso[task.setvariable variable=HasLiveTestPaths;isOutput=true]$($liveTestMatrix.Count -gt 0)"
        Write-Host "##vso[task.setvariable variable=UnitTestMatrix;isOutput=true]$(ConvertTo-Json $unitTestMatrix -Compress)"
        Write-Host "##vso[task.setvariable variable=HasUnitTestPaths;isOutput=true]$($unitTestMatrix.Count -gt 0)"
    }
}
finally {
    Pop-Location
}
