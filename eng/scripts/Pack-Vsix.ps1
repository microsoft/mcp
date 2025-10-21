[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

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

        Write-Host "Copying server icon from $($server.packageIcon) to $tempPath/resources/package-icon.png"
        New-Item -Path "$tempPath/resources" -ItemType Directory -Force | Out-Null
        Copy-Item -Path "$RepoRoot/$($server.packageIcon)" -Destination "$tempPath/resources/package-icon.png" -Force

        & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
            -Command "extract" `
            -InputReadMePath "$RepoRoot/$($server.readmePath)" `
            -PackageType "vsix" `
            -InsertPayload @{ ToolTitle = 'Extension for Visual Studio Code' } `
            -OutputDirectory $tempPath

        Write-Host "Copying NOTICE.txt and LICENSE to $tempFolder"
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force

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

            $preRelease = $setDevVersion -or $version -match '-'

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
