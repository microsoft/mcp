[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$sourcePath = "$RepoRoot/eng/vscode"
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if (!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/vsix"
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
Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

Write-Host "Copying vsix source files to $tempPath"
Copy-Item -Path $sourcePath -Destination $tempPath -Recurse -Force -ProgressAction SilentlyContinue

$originalLocation = Get-Location
Set-Location $tempPath
try {
    Write-Host "Installing npm packages"
    Invoke-LoggedCommand 'npm ci --omit=optional'

    foreach ($server in $buildInfo.servers) {
        $packagePrefix = $server.vsixPackagePrefix
        $version = $server.version

        & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
            -Command "extract" `
            -InputReadMePath "$RepoRoot/$($server.readmePath)" `
            -PackageType "vsix" `
            -InsertPayload @{ ToolTitle = 'Extension for Visual Studio Code' } `
            -OutputDirectory $tempPath

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
        Copy-Item -Path "$RepoRoot/$($server.packageIcon)" -Destination "$tempPath/resources/package-icon.png" -Force

        # Skip native platforms for now
        $filteredPlatforms = $server.platforms | Where-Object { -not $_.native }

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

            ## Update the version number
            $vsixPackageJsonPath = "./package.json"
            if (Test-Path $vsixPackageJsonPath) {
                $packageJson = Get-Content $vsixPackageJsonPath -Raw | ConvertFrom-Json
                $packageJson.version = $version
                $packageJson.name = "$packagePrefix-server"
                $packageJson.displayName = $server.assemblyTitle
                $packageJson.description = $server.vsixDescription ? $server.vsixDescription : $packageJson.description
                $packageJson.publisher = $server.vsixPublisher
                $packageJson | ConvertTo-Json -Depth 100 | Set-Content $vsixPackageJsonPath -NoNewline
                Write-Host "Updated VSIX version in $vsixPackageJsonPath to $version" -ForegroundColor Yellow
            } else {
                LogError "VSIX package.json not found at $vsixPackageJsonPath"
                $exitCode = 1
                continue
            }

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
}
finally {
    Set-Location $originalLocation
}

exit $exitCode
