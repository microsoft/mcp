#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $PublishTarget,
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

# Until https://github.com/microsoft/mcp/issues/1051 is fixed, to support hosted mcp servers, we need to ensure there
# are untrimmed versions of certain platforms available to the docker packaging step
$additionalUntrimmedPlatforms = @(
    'linux-x64'
)

if ($BuildId -eq 0) {
    if ($isPipelineRun) {
        LogError 'A non-zero BuildId is required when running in a pipeline.'
        $exitCode = 1
    } else {
        $BuildId = 99999
    }
}

if ($isPipelineRun -and !$PublishTarget) {
    LogError 'PublishTarget parameter is required when running in a pipeline.'
    $exitCode = 1
}

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

# Function to get the latest VSIX version from VS Code Marketplace
function Get-LatestMarketplaceVersion {
    param(
        [string]$PublisherId,
        [string]$ExtensionId,
        [int]$MajorVersion
    )
    
    try {
        $marketplaceUrl = "https://marketplace.visualstudio.com/_apis/public/gallery/extensionquery?api-version=7.1-preview.1"
        $body = @{
            filters = @(
                @{
                    criteria = @(
                        # filterType 7 = ExtensionName: Filter by the unique identifier (publisher.extensionName)
                        @{ filterType = 7; value = "$PublisherId.$ExtensionId" }
                    )
                }
            )
            # flags 914 = IncludeVersions | IncludeFiles | IncludeAssetUri | IncludeStatistics
            # This requests version information needed to determine the latest published version
            flags = 914
        } | ConvertTo-Json -Depth 10

        $response = Invoke-RestMethod -Uri $marketplaceUrl -Method Post -Body $body -ContentType "application/json" -ErrorAction SilentlyContinue

        if ($response.results -and $response.results[0].extensions) {
            $extension = $response.results[0].extensions[0]
            if ($extension.versions -and $extension.versions.Count -gt 0) {
                # Get all versions and filter for the target major.0.X pattern
                $allVersions = $extension.versions | ForEach-Object { $_.version }
                $matchingVersions = $allVersions | Where-Object {
                    $_ -match "^$MajorVersion\.0\.(\d+)$"
                } | ForEach-Object {
                    [PSCustomObject]@{
                        Version = $_
                        Patch = [int]$Matches[1]
                    }
                }
                
                if ($matchingVersions) {
                    $maxPatch = ($matchingVersions | Measure-Object -Property Patch -Maximum).Maximum
                    return [PSCustomObject]@{
                        LatestVersion = "$MajorVersion.0.$maxPatch"
                        MaxPatch = $maxPatch
                        NextPatch = $maxPatch + 1
                    }
                }
            }
        }
    }
    catch {
        Write-Verbose "Could not fetch marketplace versions: $_"
    }

    return $null
}

function CheckVariable($name) {
    $value = [Environment]::GetEnvironmentVariable($name)
    if (-not $value) {
        if ($isPipelineRun) {
            LogError "Environment variable $name is not set."
            $script:exitCode = 1
            return ""
        } else {
            return "Missing-$name"
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
            'tools/Azure.Mcp.Tools.KeyVault'  <-- from Microsoft.Mcp.Core's server canary list
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

        # Calculate VSIX version based on server version
        $vsixVersion = $null
        $vsixIsPrerelease = $false

        # If SETDEVVERSION is true, use BuildId as patch number (dev builds)
        if ($env:SETDEVVERSION -eq "true") {
            # VS Code Marketplace doesn't support pre-release versions with semantic versioning suffixes
            # For dev builds, we strip the prerelease label and use BuildId as patch number
            $vsixVersion = "$($version.Major).$($version.Minor).$BuildId"
            $vsixIsPrerelease = $false
            Write-Host "SETDEVVERSION is true, using BuildId as patch number for VSIX: $($version.ToString()) -> $vsixVersion" -ForegroundColor Yellow
        }
        elseif ($PublishTarget -eq 'public') {
            # Check if this is X.0.0-beta.Y series
            $isBetaSeries = $version.Minor -eq 0 -and $version.Patch -eq 0 -and $version.PrereleaseLabel -eq 'beta'
            
            if ($isBetaSeries) {
                # Map X.0.0-beta.Y -> VSIX X.0.Y (prerelease)
                $vsixVersion = "$($version.Major).$($version.Minor).$($version.PrereleaseNumber)"
                $vsixIsPrerelease = $true
            }
            else {
                # For all non-beta versions, calculate next patch version from marketplace
                $vscodePath = "$RepoRoot/servers/$serverName/vscode"
                $packageJsonPath = "$vscodePath/package.json"
                
                if (Test-Path $packageJsonPath) {
                    $packageJson = Get-Content $packageJsonPath -Raw | ConvertFrom-Json
                    $publisherId = $packageJson.publisher
                    $extensionName = $packageJson.name
                    
                    if ($publisherId -and $extensionName) {
                        Write-Host "Fetching latest marketplace version for $publisherId.$extensionName with major version $($version.Major)..." -ForegroundColor Cyan
                        
                        $marketplaceInfo = Get-LatestMarketplaceVersion -PublisherId $publisherId -ExtensionId $extensionName -MajorVersion $version.Major
                        
                        if ($marketplaceInfo) {
                            # Use next patch version from marketplace
                            $vsixVersion = "$($version.Major).0.$($marketplaceInfo.NextPatch)"
                            $vsixIsPrerelease = $false
                            Write-Host "Marketplace latest: $($marketplaceInfo.LatestVersion) -> Next VSIX version: $vsixVersion" -ForegroundColor Green
                        }
                        else {
                            # No matching versions found - this is an illegal state for non-beta releases
                            LogError "Cannot determine VSIX version for $serverName $($version.ToString()). No marketplace versions found for $($version.Major).0.X series."
                            LogError "For non-beta releases, the VSIX version must be calculated from existing marketplace versions."
                            LogError "If this is the first release for major version $($version.Major), use a beta version (e.g., $($version.Major).0.0-beta.1) instead."
                            $script:exitCode = 1
                            continue
                        }
                    }
                    else {
                        LogError "Publisher or extension name not found in $packageJsonPath for $serverName"
                        $script:exitCode = 1
                        continue
                    }
                }
                else {
                    LogError "package.json not found at $packageJsonPath for $serverName"
                    $script:exitCode = 1
                    continue
                }
            }
        }
        else {
            # For non-public builds without SETDEVVERSION, use a placeholder version
            $vsixVersion = "$($version.Major).0.999"
            $vsixIsPrerelease = $false
            Write-Host "Non-public target without SETDEVVERSION: Using placeholder VSIX version $vsixVersion" -ForegroundColor Yellow
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
                        trimmed = $true
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
                        trimmed = $false
                    }
                }

                if($additionalUntrimmedPlatforms -contains $name) {
                    $untrimmedName = "$name-untrimmed"
                    $platforms += [ordered]@{
                        name = $untrimmedName
                        artifactPath = "$serverName/$untrimmedName"
                        operatingSystem = $os.name
                        nodeOs = $os.nodeName
                        dotnetOs = $os.dotnetName
                        architecture = $arch
                        extension = $os.extension
                        native = $false
                        trimmed = $false
                        specialPurpose = 'untrimmed'
                    }
                }
            }
        }

        $serverProperties += [ordered]@{
            name = $serverProject.BaseName
            path = $serverProject | Get-RepoRelativePath -NormalizeSeparators
            artifactPath = $serverName
            version = $version.ToString()
            vsixVersion = $vsixVersion
            vsixIsPrerelease = $vsixIsPrerelease
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
        # Reduce the platform objects to unique combinations of architecture, native, and trimmed
        # Select-Object -Unique doesn't work here because we're working with hashtable
        | Sort-Object { "$($_.architecture)-$(!$_.native)-$($_.specialPurpose)" } -Descending -Unique  # x64 before arm64, non-native before native, non-special before special purpose

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

            $runUnitTests = $arch -eq 'x64' -and !$platform.native -and !$platform.specialPurpose

            $buildMatrix[$legName] = [ordered]@{
                Pool = $pool
                OSVmImage = $vmImage
                Architecture = $arch
                Native = $platform.native
                Trimmed = $platform.trimmed
                RunUnitTests = $runUnitTests
            }

            if($runUnitTests) {
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

function Get-ServerMatrix {
    param($servers)

    Write-Host "Forming server matrix"

    $serverMatrix = [ordered]@{}
    $platformName = "linux-x64-untrimmed"
    
    foreach ($server in $servers) {
        $platform = $server.platforms | Where-Object { $_.name -eq $platformName -and -not $_.native }
        $executableExtension = $platform.extension
        $imageName = $server.dockerImageName
        if (-not $platform.extension) { $executableExtension = '' }
        if (-not $server.dockerImageName) { $imageName = "microsoft/" + $server.cliName + "-mcp" }
        $serverMatrix[$server.name] = [ordered]@{
            ServerName = $server.name
            CliName = $server.cliName
            ArtifactPath = $server.artifactPath
            Platform = $platformName
            Version = $server.version
            ImageName = $imageName
            ExecutableName = $server.cliName + $executableExtension
            DockerLocalTag = $imageName + ":" + $BuildId
        }
    }

    return $serverMatrix
}

Push-Location $RepoRoot
try {
    $serverDetails = @(Get-ServerDetails)
    $matrices = Get-BuildMatrices $serverDetails
    $pathsToTest = @(Get-PathsToTest)
    $matrices['liveTestMatrix'] = Get-TestMatrix $pathsToTest -TestType 'Live'
    $matrices['serverMatrix'] = Get-ServerMatrix $serverDetails

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
    }
}
finally {
    Pop-Location
}

exit $exitCode
