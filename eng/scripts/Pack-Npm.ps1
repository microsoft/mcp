#!/bin/env pwsh
#Requires -Version 7

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath,
    [switch] $UsePaths
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$wrapperSourcePath = "$RepoRoot/eng/npm/wrapper"
$platformSourcePath = "$RepoRoot/eng/npm/platform"

# When running locally, ignore missing artifacts instead of failing
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if(!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if(!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if(!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_npm"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

if(!(Test-Path $ArtifactsPath)) {
    LogError "Artifacts path $ArtifactsPath does not exist."
    $exitCode = 1
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

$tempFolder = "$RepoRoot/.work/temp"

function BuildServerPackages([hashtable] $server, [bool] $native) {
    $serverDirectory = "$ArtifactsPath/$($server.artifactPath)"

    if(!(Test-Path $serverDirectory)) {
        $message = "Server directory $serverDirectory does not exist."
        if ($ignoreMissingArtifacts) {
            Write-Warning $message
        } else {
            Write-Error $message
        }
        return
    }

    $filteredPlatforms = $server.platforms | Where-Object { $_.native -eq $native -and -not $_.specialPurpose }
    if ($filteredPlatforms.Count -eq 0) {
        Write-Host "No platforms to build for server $($server.name) with native=$native"
        return
    }

    $serverOutputPath = "$OutputPath/$($server.artifactPath)"

    $wrapperOutputPath = "$serverOutputPath/wrapper"
    New-Item -ItemType Directory -Force -Path $wrapperOutputPath | Out-Null

    $platformOutputPath = "$serverOutputPath/platform"
    New-Item -ItemType Directory -Force -Path $platformOutputPath | Out-Null

    $packageName = $server.npmPackageName
    $description = $server.npmDescription ? $server.npmDescription : $server.description
    $cliName = $server.cliName
    $keywords = @($server.npmPackageKeywords)

    if ($native) {
        $packageName += "-native"
        $description += " with native dependencies"
        $keywords += "native"
    }

    $wrapperPackage = [ordered]@{
        name = $packageName
        version = $server.version
        description = $description
        author = 'Microsoft'
        homepage = $server.readmeUrl
        license = 'MIT'
        keywords = $keywords
        bugs = @{ url = "https://github.com/microsoft/mcp/issues" }
        repository = @{ type = 'git'; url = 'https://github.com/microsoft/mcp.git' }
        engines = @{ node = '>=20.0.0' }
        bin = @{ $cliName = './index.js' }
        os = @()
        cpu = @()
        optionalDependencies = @{}
        scripts = @{ postinstall = "node ./scripts/post-install-script.js" }
    }

    # Build the project
    foreach ($platform in $filteredPlatforms) {
        $platformDirectory = "$ArtifactsPath/$($platform.artifactPath)"

        if(!(Test-Path $platformDirectory)) {
            $errorMessage = "Platform directory $platformDirectory does not exist."
            if ($ignoreMissingArtifacts) {
                Write-Warning $errorMessage
                continue
            }

            Write-Error $errorMessage
            return
        }

        $nodeOs = $platform.nodeOs
        $arch = $platform.architecture
        $platformPackageName = "$packageName-$nodeOs-$arch"

        $extension = $platform.extension
        $binPath = "dist/$cliName$extension"

        Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $tempFolder | Out-Null

        Write-Host "Copying $packageName platform files from $platformDirectory to $tempFolder/dist"
        Copy-Item -Path $platformDirectory -Destination "$tempFolder/dist" -Recurse -Force

        Write-Host "Copying platform script files from $platformSourcePath to $tempFolder"
        Copy-Item -Path "$platformSourcePath/*" -Destination $tempFolder -Force

        # Remove symbols files before packing
        Write-Host "Removing symbol files from $tempFolder"
        Get-ChildItem -Path $tempFolder -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        $platformPackage = [ordered]@{
            name = $platformPackageName
            version = $server.version
            description = "$description, for $nodeOs on $arch"
            author = 'Microsoft'
            homepage = $server.readmeUrl
            license = 'MIT'
            keywords = $keywords
            bugs = @{ url = "https://github.com/microsoft/mcp/issues" }
            repository = @{ type = 'git'; url = 'https://github.com/microsoft/mcp.git' }
            engines = @{ node = '>=20.0.0' }
            main = './index.js'
            bin = @{ "$cliName-$nodeOs-$arch" = "./$binPath" }
            os = @($nodeOs)
            cpu = @($arch)
        }

        if($wrapperPackage.os -notcontains $nodeOs) {
            $wrapperPackage.os += $nodeOs
        }

        if($wrapperPackage.cpu -notcontains $arch) {
            $wrapperPackage.cpu += $arch
        }

        if (!$IsWindows) {
            Write-Host "Setting executable permissions for $tempFolder/index.js" -ForegroundColor Yellow
            Invoke-LoggedCommand "chmod +x `"$tempFolder/index.js`""

            if ($os -ne 'win32') {
                Write-Host "Setting executable permissions for $tempFolder/$binPath" -ForegroundColor Yellow
                Invoke-LoggedCommand "chmod +x `"$tempFolder/$binPath`""
            }
        }
        else {
            Write-Warning "Executable permissions are not set when packing on a Windows agent."
        }

        $platformFile = "$tempFolder/package.json"
        $platformPackageJson = $platformPackage | ConvertTo-Json -Depth 10
        Write-Host "Writing $platformFile with contents:`n$platformPackageJson"
        $platformPackageJson | Out-File -FilePath $platformFile -Encoding utf8 -Force

        & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
            -Command "extract" `
            -InputReadMePath "$RepoRoot/$($server.readmePath)" `
            -PackageType "npm" `
            -InsertPayload @{ ToolTitle = 'NPM Package' } `
            -OutputDirectory $tempFolder

        Write-Host "Copying README.md, NOTICE.txt and LICENSE to $tempFolder"
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force

        Write-Host "Packaging $tempFolder into $platformOutputPath"
        Invoke-LoggedCommand "npm pack $tempFolder --pack-destination '$platformOutputPath'" -GroupOutput | Tee-Object -Variable fileName
        Write-Host "Package location: $platformOutputPath/$fileName" -ForegroundColor Yellow

        if ($UsePaths) {
            $wrapperPackage.optionalDependencies[$platformPackage.name] = "file://$((Resolve-Path "$platformOutputPath/$fileName").Path.Replace('\', '/'))"
        } else {
            $wrapperPackage.optionalDependencies[$platformPackage.name] = $platformPackage.version
        }
    }

    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    Copy-Item -Path $wrapperSourcePath -Destination $tempFolder -Recurse -Force
    Write-Host "Copied wrapper script files into $tempFolder"

    if (!$IsWindows) {
        Write-Host "Setting executable permissions for $tempFolder/index.js" -ForegroundColor Yellow
        Invoke-LoggedCommand "chmod +x `"$tempFolder/index.js`""
    }

    $wrapperFile = "$tempFolder/package.json"
    $wrapperPackageJson = $wrapperPackage | ConvertTo-Json -Depth 10
    Write-Host "Writing $wrapperFile with contents:`n$wrapperPackageJson"
    $wrapperPackageJson | Out-File -FilePath "$tempFolder/package.json" -Encoding utf8

    Write-Host "Copying README.md and LICENSE to $tempFolder"
    Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force


    & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
        -Command "extract" `
        -InputReadMePath "$RepoRoot/$($server.readmePath)" `
        -PackageType "npm" `
        -InsertPayload @{ ToolTitle = 'NPM Package' } `
        -OutputDirectory $tempFolder

    Write-Host "Packaging $tempFolder into $wrapperOutputPath"
    Invoke-LoggedCommand "npm pack $tempFolder --pack-destination '$wrapperOutputPath'" -GroupOutput | Tee-Object -Variable fileName
    Write-Host "Package location: $wrapperOutputPath/$fileName" -ForegroundColor Yellow
}

Push-Location $RepoRoot
try {
    foreach($server in $buildInfo.servers) {
        BuildServerPackages $server -native $false

        # Until we want to ship native packages, we won't build them.
        # if($server.platforms | Where-Object { $_.native }) {
        #     BuildServerPackages $server -native $true
        # }
    }

    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    Write-Host "`nPackaging completed successfully!" -ForegroundColor Green
}
finally {
    Pop-Location
}

exit $exitCode
