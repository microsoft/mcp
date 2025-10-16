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
    [switch] $TestPipeline,
    [switch] $CI
)

. "$PSScriptRoot/../common/scripts/common.ps1"
. "$PSScriptRoot/helpers/PathHelpers.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$isPipelineRun = $CI -or $env:TF_BUILD -eq 'true'
$exitCode = 0

# We currently only want to build linux-x64 native
$nativePlatforms = @(
    'linux-x64-native'
    #'linux-arm64-native'  We can't currently build arm64 native in a CI run
    #'macos-x64-native'
    #'macos-arm64-native'
    #'windows-x64-native'
    #'windows-arm64-native'
)

# When native builds are shipped, we still may want to build only linux-x64 native in pull requests for pipeline performance
# if ($isPipelineRun -and $PublishTarget -eq 'none') {
#     $nativePlatforms = @('linux-x64-native')
# }

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/build_info.json"
}

$serverDirectories = Get-ChildItem "$RepoRoot/servers" -Directory
$toolDirectories = Get-ChildItem "$RepoRoot/tools" -Directory
$coreDirectories = Get-ChildItem "$RepoRoot/core" -Directory

$architectures = @('x64', 'arm64')

$operatingSystems = @(
    @{ name = 'linux'; nodeName = 'linux'; dotnetName = 'linux'; extension = '' }
    @{ name = 'macos'; nodeName = 'darwin'; dotnetName = 'osx'; extension = '' }
    @{ name = 'windows'; nodeName = 'win32'; dotnetName = 'win'; extension = '.exe' }
)

# Public releases always use the version from the repo without a dynamic prerelease suffix, except for test pipelines
# which always use a dynamic prerelease suffix to allow for multiple releases from the same commit
$dynamicPrereleaseVersion = $PublishTarget -ne 'public' -or $TestPipeline

function CheckVariable($name) {
    $value = [Environment]::GetEnvironmentVariable($name)
    if (-not $value) {
        if ($isPipelineRun) {
            LogError "Environment variable $name is not set."
            $script:exitCode = 1
            return ""
        } else {
            $substitute = "Missing-$name"
            LogWarning "Environment variable $name is not set. Using substitute value '$substitute'." -ForegroundColor Yellow
            return $substitute
        }
    }
    return $value
}

$windowsPool = CheckVariable 'WINDOWSPOOL'
$linuxPool = CheckVariable 'LINUXPOOL'
$macPool = CheckVariable 'MACPOOL'

$windowsVmImage = CheckVariable 'WINDOWSVMIMAGE'
$linuxVmImage = CheckVariable 'LINUXVMIMAGE'
$macVmImage = CheckVariable 'MACVMIMAGE'

function Get-PathsToTest {
    Write-Host "Getting paths to test"

    # When "core" is modified, include storage and keyVault as the canary service tools.
    # TODO: These should be sourced from csproj files
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
        Write-Host "Filtering list of test paths using project references for $serverName"
        $serverProject = "$RepoRoot/servers/$ServerName/src/$ServerName.csproj"
        if (-not (Test-Path $serverProject)) {
            LogError "No project for $ServerName found at $serverProject"
            $script:exitCode = 1
            return @()
        }

        $projectReferences = (dotnet build $serverProject -getItem:ProjectReference | ConvertFrom-Json).Items.ProjectReference.FullPath

        # We can put full paths here because they'll be reduced to relative project directory paths in the "reduce down" step below
        @() + $serverProject + $projectReferences
    } else {
        @() + $coreDirectories + $serverDirectories + $toolDirectories
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
        | ForEach-Object { $_ -match $projectDirectoryPattern -and $normalizedPaths -contains $Matches[0] ? $Matches[0] : 'core/Microsoft.Mcp.Core' }
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
    Write-Host "Getting server details"
    $searchDirectories = $serverDirectories

    if ($ServerName) {
        $searchDirectories = $serverDirectories | Where-Object { $_.Name -ieq $ServerName }
        if ($searchDirectories.Count -eq 0) {
            LogError "No server directory found with name $ServerName in $RepoRoot/servers."
            $script:exitCode = 1
            return @()
        }
    }

    $serverProjects = $searchDirectories | Get-ChildItem -Filter "src/*.csproj"

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
                $nativeName = "$name-native"

                if ($excludedPlatforms -notcontains $name) {
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
                }

                $shouldIncludeNative = $IncludeNative -and
                    $props.IsAotCompatible -eq 'true' -and
                    $nativePlatforms -contains $nativeName

                if($shouldIncludeNative) {
                    $platforms += [ordered]@{
                        name = $nativeName
                        artifactPath = "$serverName/$nativeName"
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
            path = $serverProject | Get-RepoRelativePath -NormalizeSeparators
            artifactPath = $serverName
            version = $version.ToString()
            releaseTag = "$serverName-$version"
            cliName = $props.CliName
            assemblyTitle = $props.AssemblyTitle
            description = $props.Description
            readmeUrl = $props.ReadmeUrl
            readmePath = $props.ReadmePath | Get-RepoRelativePath -NormalizeSeparators
            packageIcon = $props.PackageIcon | Get-RepoRelativePath -NormalizeSeparators
            npmPackageName = $props.NpmPackageName
            npmDescription = $props.NpmDescription
            npmPackageKeywords = @($props.NpmPackageKeywords -split '[;,] *' | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' })
            dockerImageName = $props.DockerImageName
            dockerDescription = $props.DockerDescription
            dnxPackageId = $props.DnxPackageId
            dnxDescription = $props.DnxDescription
            dnxToolCommandName = $props.DnxToolCommandName
            dnxPackageTags = @($props.DnxPackageTags -split '[;,] *' | ForEach-Object { $_.Trim() } | Where-Object { $_ -ne '' })
            vsixPackagePrefix = $props.VsixPackagePrefix
            vsixDescription = $props.VsixDescription
            vsixPublisher = $props.VsixPublisher
            platforms = $platforms
        }
    }

    return $serverProperties
}

function Get-BuildMatrices {
    param($servers)

    Write-Host "Forming build matrices"
    $matrices = [ordered]@{}

    foreach ($os in $operatingSystems.name) {
        $buildMatrix = [ordered]@{}
        $smokeTestMatrix = [ordered]@{}

        $supportedPlatforms = $servers.platforms
        | Where-Object { $_.operatingSystem -eq $os }
        | Sort-Object { "$($_.architecture)-$(!$_.native)" } -Unique -Descending # x64 before arm64, non-native before native

        foreach($platform in $supportedPlatforms) {
            $arch = $platform.architecture
            $legName = $platform.name -replace '\W', '_' # e.g. linux-arm64 or windows-x64-native

            if ($excludedPlatforms -contains $platform.name) {
                Write-Host "Excluding build leg $legName"
                continue
            }

            $pool = switch($os) {
                'windows' { $windowsPool }
                'linux' { $linuxPool }
                'macos' { $macPool }
            }

            $vmImage = switch($os) {
                'windows' { $windowsVmImage }
                'linux' { $linuxVmImage }
                'macos' { $macVmImage }
            }

            $buildMatrix[$legName] = [ordered]@{
                Pool = $pool
                OSVmImage = $vmImage
                Architecture = $arch
                Native = $platform.native
                RunUnitTests = $arch -eq 'x64' -and -not $platform.native
            }

            if(!$platform.Native -and $arch -eq 'x64') {
                $smokeTestMatrix[$legName] = [ordered]@{
                    Pool = $pool
                    OSVmImage = $vmImage
                    Architecture = $arch
                }
            }
        }

        $matrices["${os}BuildMatrix"] = $buildMatrix
        $matrices["${os}SmokeTestMatrix"] = $smokeTestMatrix
    }

    return $matrices
}

Push-Location $RepoRoot
try {
    $serverDetails = @(Get-ServerDetails)
    $matrices = Get-BuildMatrices $serverDetails
    $pathsToTest = @(Get-PathsToTest)
    $matrices['liveTestMatrix'] = Get-TestMatrix $pathsToTest -TestType 'Live'

    # spellchecker: ignore SOURCEVERSION
    $branch = $isPipelineRun ? (CheckVariable 'BUILD_SOURCEBRANCH') : (git rev-parse --abbrev-ref HEAD)
    $commitSha = $isPipelineRun ? (CheckVariable 'BUILD_SOURCEVERSION') : (git rev-parse HEAD)

    $buildInfo = [ordered]@{
        buildId = $BuildId
        publishTarget = $PublishTarget
        dynamicPrereleaseVersion = $dynamicPrereleaseVersion
        repositoryUrl = 'https://github.com/microsoft/mcp'
        branch = $branch
        commitSha = $commitSha
        servers = $serverDetails
        pathsToTest = $pathsToTest
        matrices = $matrices
    }

    Write-Host "Writing build info to $OutputPath"
    $parentDirectory = Split-Path $OutputPath -Parent
    New-Item -Path $parentDirectory -ItemType Directory -Force | Out-Null

    $buildInfo | ConvertTo-Json -Depth 5 | Out-File -FilePath $OutputPath -Encoding utf8 -Force

    if ($isPipelineRun) {
        foreach($key in $matrices.Keys) {
            $matrixJson = $matrices[$key] | ConvertTo-Json -Compress
            Write-Host "##vso[task.setvariable variable=${key};isOutput=true]$matrixJson"
        }
        
        $serverDetailsJson = $serverDetails | ConvertTo-Json -Compress -Depth 5
        Write-Host "##vso[task.setvariable variable=AllServerDetails;isOutput=true]$serverDetailsJson"
    }
}
finally {
    Pop-Location
}

exit $exitCode
