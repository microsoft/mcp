[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

# Function to get the latest VSIX version from VS Code Marketplace
function Get-LatestMarketplaceVersion {
    param(
        [string]$PublisherId,
        [string]$ExtensionId
    )
    
    try {
        $marketplaceUrl = "https://marketplace.visualstudio.com/_apis/public/gallery/extensionquery"
        $body = @{
            filters = @(
                @{
                    criteria = @(
                        @{ filterType = 7; value = "$PublisherId.$ExtensionId" }
                    )
                }
            )
            flags = 914
        } | ConvertTo-Json -Depth 10
        
        $response = Invoke-RestMethod -Uri $marketplaceUrl -Method Post -Body $body -ContentType "application/json" -ErrorAction SilentlyContinue
        
        if ($response.results -and $response.results[0].extensions) {
            $extension = $response.results[0].extensions[0]
            if ($extension.versions -and $extension.versions.Count -gt 0) {
                # Get all versions and find the highest X.0.Y version for the given major version
                $allVersions = $extension.versions | ForEach-Object { $_.version }
                return $allVersions
            }
        }
    }
    catch {
        Write-Verbose "Could not fetch marketplace versions: $_"
    }
    
    return $null
}

$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if (!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_vsix"
}

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if(!(Test-Path $ArtifactsPath)) {
    LogError "Artifacts path $ArtifactsPath does not exist."
    $exitCode = 1
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info path $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 first."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json

if(Test-Path $OutputPath) {
    Write-Host "Cleaning existing output path $OutputPath"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
}

$tempPath = "$RepoRoot/.work/temp"

foreach ($server in $buildInfo.servers) {
    $vsixDirectory = "$RepoRoot/servers/$($server.name)/vscode"

    if(!(Test-Path $vsixDirectory)) {
        LogWarning "VSIX directory $vsixDirectory does not exist."
        continue
    }

    Write-Host "Packing VSIX for server $($server.name)"

    Write-Host "Creating clean temporary working directory $tempPath"
    Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $tempPath | Out-Null

    $originalLocation = Get-Location
    Set-Location $tempPath
    try {
        Write-Host "Copying VSIX base files from $vsixDirectory to $tempPath"
        Copy-Item -Path "$vsixDirectory/*" -Destination $tempPath -Recurse -Force -ProgressAction SilentlyContinue

        Write-Host "Installing npm packages"
        Invoke-LoggedCommand 'npm ci --omit=optional'

        $version = $server.version
        
        # Check if this is X.0.0 series (beta or GA) before any version transformations
        $semverOriginal = [AzureEngSemanticVersion]::new($version)
        $isBetaSeries = $semverOriginal.Minor -eq 0 -and $semverOriginal.Patch -eq 0 -and $semverOriginal.PrereleaseLabel -eq 'beta'
        $isGA = $semverOriginal.Minor -eq 0 -and $semverOriginal.Patch -eq 0 -and !$semverOriginal.IsPrerelease

        # If not SetDevVersion, don't strip pre-release labels leaving the packages unpublishable
        if($env:SETDEVVERSION -eq "true") {
            <#
                VS Code Marketplace doesn't support pre-release versions. Also, the major.minor.patch portion of the
                version number is stored in the repo, making the pre-release suffix the only dynamic portion of the
                version.

                In build runs with "SetDevVersion" set to true, we are intentionally publishing dynamically
                numbered packages to the marketplace. In this case, we use the CI build id as the patch number
                (e.g. 1.2.3 -> 1.2.56789)
            #>
            $semver = [AzureEngSemanticVersion]::new($version)
            $semver.PrereleaseLabel = ''
            $semver.Patch = $buildInfo.buildId
            $version = $semver.ToString()
            Write-Host "SetDevVersion is true, using Build.BuildId as patch number: $($server.version) -> $version" -ForegroundColor Yellow
        }
        else {
            <#
                For VSIX releases, handle version mapping for X.0.0 series (any major version).
                
                Strategy:
                - X.0.0-beta.Y -> VSIX X.0.Y (pre-release) - Auto-mapped from beta number
                - X.0.0 (GA)   -> VSIX X.0.Z (GA) - Read from package.json
                
                For GA releases:
                1. VSIX version must be set in package.json (e.g., "version": "2.0.6")
                2. Version must be valid semantic version without prerelease labels (no -alpha, -beta, etc.)
                3. Major.Minor must match the pattern X.0 where X matches Azure.Mcp.Server major version
                
                Other versions (X.Y.Z where Y!=0 or Z!=0) pass through unchanged.
            #>
            if ($isBetaSeries) {
                # Map X.0.0-beta.Y -> VSIX X.0.Y (with pre-release flag)
                $originalVersion = $version
                $semver = [AzureEngSemanticVersion]::new($version)
                $semver.Patch = $semver.PrereleaseNumber
                $semver.PrereleaseLabel = ''
                $version = $semver.ToString()
                Write-Host "Beta version mapping: $originalVersion -> VSIX $version (pre-release)" -ForegroundColor Cyan
            }
            elseif ($isGA) {
                # X.0.0 GA release - read VSIX version from package.json and validate
                $originalVersion = $version
                
                # Read package.json to get the VSIX GA version
                $packageJsonPath = "./package.json"
                if (!(Test-Path $packageJsonPath)) {
                    Write-Host "ERROR: package.json not found at $packageJsonPath" -ForegroundColor Red
                    $script:exitCode = 1
                    continue
                }
                
                $packageJson = Get-Content $packageJsonPath -Raw | ConvertFrom-Json
                $vsixVersion = $packageJson.version
                
                # Validate that package.json version is a valid GA version (not alpha/beta/prerelease)
                if ($vsixVersion -match '-') {
                    Write-Host "ERROR: package.json version ($vsixVersion) contains prerelease label. For GA releases, version must not contain '-alpha', '-beta', or other prerelease labels." -ForegroundColor Red
                    Write-Host "Please update package.json version to a GA version (e.g., $($semverOriginal.Major).0.6)" -ForegroundColor Yellow
                    $script:exitCode = 1
                    continue
                }
                
                # Parse and validate the VSIX version format
                if ($vsixVersion -notmatch '^(\d+)\.(\d+)\.(\d+)$') {
                    Write-Host "ERROR: package.json version ($vsixVersion) is not in valid semantic version format (X.Y.Z)" -ForegroundColor Red
                    $script:exitCode = 1
                    continue
                }
                
                $vsixMajor = [int]$Matches[1]
                $vsixMinor = [int]$Matches[2]
                
                # Verify the major and minor versions match Azure.Mcp.Server version
                if ($vsixMajor -ne $semverOriginal.Major -or $vsixMinor -ne 0) {
                    Write-Host "ERROR: package.json version ($vsixVersion) major.minor does not match Azure.Mcp.Server version pattern $($semverOriginal.Major).0.X" -ForegroundColor Red
                    Write-Host "For $originalVersion GA release, VSIX version should be $($semverOriginal.Major).0.X" -ForegroundColor Yellow
                    $script:exitCode = 1
                    continue
                }
                
                # Use the version from package.json
                $version = $vsixVersion
                Write-Host "GA version mapping: $originalVersion -> VSIX $version (from package.json)" -ForegroundColor Green
            }
            # All other versions (X.Y.Z where Y!=0 or Z!=0) pass through unchanged
        }

        Write-Host "Copying server icon from $($server.packageIcon) to $tempPath/resources/package-icon.png"
        New-Item -Path "$tempPath/resources" -ItemType Directory -Force | Out-Null
        Copy-Item -Path "$RepoRoot/$($server.packageIcon)" -Destination "$tempPath/resources/package-icon.png" -Force

        & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
            -Command "extract" `
            -InputReadMePath "$RepoRoot/$($server.readmePath)" `
            -PackageType "vsix" `
            -InsertPayload @{ ToolTitle = 'Extension for Visual Studio Code' } `
            -OutputDirectory $tempPath

        Write-Host "Copying NOTICE.txt and LICENSE to $tempPath"
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempPath -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempPath -Force

        # Skip native platforms for now
        $filteredPlatforms = $server.platforms | Where-Object { -not $_.native }

        ## Update the version number
        $vsixPackageJsonPath = "./package.json"
        if (Test-Path $vsixPackageJsonPath) {
            $packageJson = Get-Content $vsixPackageJsonPath -Raw | ConvertFrom-Json
            $packageJson.version = $version
            $packageJson | ConvertTo-Json -Depth 100 | Set-Content $vsixPackageJsonPath -NoNewline

            $packagePrefix = $packageJson.name -replace '-server$', ''
            Write-Host "Updated VSIX version in $vsixPackageJsonPath to $version" -ForegroundColor Yellow
        } else {
            LogError "VSIX package.json not found at $vsixPackageJsonPath"
            $exitCode = 1
            continue
        }

        foreach ($platform in $filteredPlatforms) {
            $platformDirectory = "$ArtifactsPath/$($platform.artifactPath)"

            if(!(Test-Path $platformDirectory)) {
                $message = "Platform directory $platformDirectory does not exist."
                if ($ignoreMissingArtifacts) {
                    LogWarning $message
                } else {
                    LogError $message
                    $script:exitCode = 1
                }

                continue
            }

            $os = $platform.nodeOs
            $cpu = $platform.architecture
            $target = "$os-$cpu" # Node name, e.g. darwin-arm64
            $platformName = $platform.name # Pipeline platform name, e.g. osx-arm64
            $vsixBaseName = "$packagePrefix-extension-$target-$version"
            $outputDirectory = "$OutputPath/$($platform.artifactPath)"
            $vsixPath = "$outputDirectory/$vsixBaseName.vsix"
            $manifestPath = "$outputDirectory/$vsixBaseName.manifest"
            $signaturePath = "$outputDirectory/$vsixBaseName.signature.p7s"

            Write-Host @"

====================================================================================
Processing VSIX packaging: $vsixBaseName
  Platform: $platformName
  Target: $target
  Version: $version
  Binaries Path: $platformDirectory
  Vsix Path: $vsixPath
  Manifest Path: $manifestPath
  Signature Path: $signaturePath
====================================================================================

"@

            Write-Host "Cleaning $tempPath/server"
            Remove-Item -Path "$tempPath/server" -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

            Write-Host "Copying $platformDirectory to $tempPath/server"
            Copy-Item -Path $platformDirectory -Destination "$tempPath/server" -Recurse -Force -ProgressAction SilentlyContinue

            # Remove symbols files before packing
            Get-ChildItem -Path "$tempPath/server" -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

            New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

            # Determine pre-release status: SetDevVersion, X.0.0-beta.Y series, or version contains dash
            # Note: For X.0.0 GA (no beta label), $isBetaSeries will be $false, so --pre-release won't be used
            $preRelease = $isBetaSeries -or $setDevVersion -or $version -match '-'

            ## Run package command
            Write-Host "Packaging $vsixBaseName"
            Invoke-LoggedCommand "npx --no @vscode/vsce package --target $target --out $vsixPath --ignoreFile .vscodeignore $($preRelease ? '--pre-release' : '')" | Out-Host

            ## Create manifest
            Write-Host "Generating signing manifest for $vsixBaseName"
            Invoke-LoggedCommand "npx --no '@vscode/vsce' generate-manifest --packagePath '$vsixPath' -o '$manifestPath'" | Out-Host

            ## Prepare signature file
            Write-Host "Preparing signature file for $vsixBaseName"
            Copy-Item -Path $manifestPath -Destination $signaturePath -Force
        }
    }
    finally {
        Set-Location $originalLocation
    }
}

exit $exitCode
