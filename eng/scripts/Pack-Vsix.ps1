[CmdletBinding()]
param(
    [string] $ServerName,
    [string] $ArtifactsPath,
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')
. "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1"

$sourcePath = "$RepoRoot/eng/vscode"

$buildId = $env:BUILD_BUILDID
$setDevVersion = $env:SETDEVVERSION -eq "true" # spellchecker: ignore SETDEVVERSION

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/vsix"
}

if(!(Test-Path $ArtifactsPath)) {
    Write-Error "Artifacts path $ArtifactsPath does not exist."
    exit 1
}

$serverJsonFiles = Get-ChildItem -Path $ArtifactsPath -Filter "wrapper.json" -Recurse

if ($ServerName) {
    $serverJsonFiles = $serverJsonFiles | Where-Object { $_.Directory.Name -ieq $ServerName }
    if ($serverJsonFiles.Count -eq 0) {
        Write-Error "No server directory found with name $ServerName in $ArtifactsPath."
        exit 1
    }
}

if(Test-Path $OutputPath) {
    Write-Host "Cleaning existing output path $OutputPath"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
}

$tempPath = "$RepoRoot/.work/temp"
if (Test-Path $tempPath) {
    Write-Host "Cleaning existing temp path $tempPath"
    Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

Write-Host "Copying vsix source files to $tempPath"
Copy-Item -Path $sourcePath -Destination $tempPath -Recurse -Force -ProgressAction SilentlyContinue

Push-Location $tempPath
try {
    Write-Host "Installing npm packages"
    Invoke-LoggedCommand 'npm ci --omit=optional'

    foreach ($serverJsonFile in $serverJsonFiles) {
        $serverJson = Get-Content $serverJsonFile -Raw | ConvertFrom-Json
        $version = $serverJson.version
        $serverDirectory = $serverJsonFile.Directory
        $serverName = $serverDirectory.Name

        Extract-PackageSpecificReadMe -InputReadMePath "$serverDirectory/README.md" `
            -OutputDirectory $tempPath -PackageType "vsix" -InsertPayload @{ ToolTitle = 'Extension for Visual Studio Code' }

        if($setDevVersion -and $buildId) {
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
            $semver.Patch = $buildId
            $version = $semver.ToString()
            Write-Host "SetDevVersion is true, using Build.BuildId as patch number: $($serverJson.version) -> $version" -ForegroundColor Yellow
        }

        # If not SetDevVersion, don't strip pre-release labels leaving the packages unpublishable

        $platformDirectories = Get-ChildItem $serverDirectory -Directory
        foreach ($platformDirectory in $platformDirectories) {
            $platformJson = Get-Content "$platformDirectory/package.json" -Raw | ConvertFrom-Json

            $os = $platformJson.os[0]
            $cpu = $platformJson.cpu[0]
            $target = "$os-$cpu" # Node name, e.g. darwin-arm64
            $platformName = $platformDirectory.Name # Pipeline platform name, e.g. osx-arm64
            $vsixBaseName = "vscode-azure-mcp-extension-$target-$version"
            $outputDirectory = "$OutputPath/$serverName/$platformName"
            $vsixPath = "$outputDirectory/$vsixBaseName.vsix"
            $manifestPath = "$outputDirectory/$vsixBaseName.manifest"
            $signaturePath = "$outputDirectory/$vsixBaseName.signature.p7s"

            Write-Host @"

====================================================================================
Processing VSIX packaging: $vsixBaseName
  Platform: $platformName
  Target: $target
  Version: $version
  Vsix Path: $vsixPath
  Manifest Path: $manifestPath
  Signature Path: $signaturePath
====================================================================================

"@

            if (Test-Path "$tempPath/server") {
                Write-Host "Cleaning $tempPath/server"
                Remove-Item -Path "$tempPath/server" -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
            }

            Write-Host "Copying $platformDirectory/dist to $tempPath/server"
            Copy-Item -Path "$platformDirectory/dist" -Destination "$tempPath/server" -Recurse -Force -ProgressAction SilentlyContinue

            # Remove symbols files before packing
            Get-ChildItem -Path "$tempPath/server" -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue

            New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

            ## Update the version number
            $vsixPackageJsonPath = "./package.json"
            if (Test-Path $vsixPackageJsonPath) {
                $packageJson = Get-Content $vsixPackageJsonPath -Raw | ConvertFrom-Json
                $packageJson.version = $version
                $packageJson | ConvertTo-Json -Depth 100 | Set-Content $vsixPackageJsonPath -NoNewline
                Write-Host "Updated VSIX version in $vsixPackageJsonPath to $version" -ForegroundColor Yellow
            } else {
                Write-Error "VSIX package.json not found at $vsixPackageJsonPath"
                exit 1
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
    Pop-Location
}
