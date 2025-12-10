#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Packs Azure MCP Server binaries into PyPI packages for distribution.

.DESCRIPTION
    This script creates PyPI packages for the Azure MCP Server, similar to the npm package structure.
    It creates:
    - A wrapper package (e.g., azmcp) that detects the platform and delegates to platform-specific packages
    - Platform-specific packages (e.g., azmcp-win32-x64, azmcp-darwin-arm64) containing the actual binaries

    Users can install with:
    - pip install azmcp
    - uvx azmcp

.PARAMETER ArtifactsPath
    Path to the build artifacts containing the server binaries.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and platform details.

.PARAMETER OutputPath
    Path where the PyPI packages will be created.

.PARAMETER UsePaths
    Switch to use default paths for local development.

.EXAMPLE
    ./Pack-Pypi.ps1 -UsePaths
    Creates PyPI packages using default local paths.

.EXAMPLE
    ./Pack-Pypi.ps1 -ArtifactsPath ".work/build" -BuildInfoPath ".work/build_info.json"
    Creates PyPI packages using specified artifact and build info paths.
#>

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

$wrapperSourcePath = "$RepoRoot/eng/pypi/wrapper"
$platformSourcePath = "$RepoRoot/eng/pypi/platform"

# When running locally, ignore missing artifacts instead of failing
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if (!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_pypi"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

if (!(Test-Path $ArtifactsPath)) {
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

$tempFolder = "$RepoRoot/.work/temp_pypi"

# Map node OS names to PyPI OS names
$osNameMap = @{
    'win32'  = 'win32'
    'darwin' = 'darwin'
    'linux'  = 'linux'
}

# Map OS names to Python classifier OS names
$osClassifierMap = @{
    'win32'  = 'Microsoft :: Windows'
    'darwin' = 'MacOS'
    'linux'  = 'POSIX :: Linux'
}

function Get-ModuleName($packageName) {
    return $packageName.Replace('-', '_')
}

function Get-KeywordsString($keywords) {
    return ($keywords | ForEach-Object { "`"$_`"" }) -join ', '
}

function BuildServerPackages([hashtable] $server, [bool] $native) {
    $serverDirectory = "$ArtifactsPath/$($server.artifactPath)"

    if (!(Test-Path $serverDirectory)) {
        $message = "Server directory $serverDirectory does not exist."
        if ($ignoreMissingArtifacts) {
            Write-Warning $message
        }
        else {
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

    # Use npm package name pattern but for PyPI (without @scope/)
    $basePackageName = $server.pypiPackageName ?? $server.cliName
    $description = $server.pypiDescription ?? $server.description
    $cliName = $server.cliName
    $keywords = @($server.pypiPackageKeywords ?? $server.npmPackageKeywords)

    if ($native) {
        $basePackageName += "-native"
        $description += " with native dependencies"
        $keywords += "native"
    }

    # Track platform dependencies for wrapper package
    $platformDependencies = @{}
    $supportedPlatforms = @()

    # Build the platform packages
    foreach ($platform in $filteredPlatforms) {
        $platformDirectory = "$ArtifactsPath/$($platform.artifactPath)"

        if (!(Test-Path $platformDirectory)) {
            $errorMessage = "Platform directory $platformDirectory does not exist."
            if ($ignoreMissingArtifacts) {
                Write-Warning $errorMessage
                continue
            }

            Write-Error $errorMessage
            return
        }

        $pypiOs = $osNameMap[$platform.nodeOs]
        $arch = $platform.architecture
        $platformPackageName = "$basePackageName-$pypiOs-$arch"
        $moduleName = Get-ModuleName $platformPackageName

        $extension = $platform.extension
        $binPath = "bin/$cliName$extension"

        Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $tempFolder | Out-Null
        New-Item -ItemType Directory -Force -Path "$tempFolder/src/$moduleName/bin" | Out-Null

        Write-Host "Copying $platformPackageName platform files from $platformDirectory to $tempFolder/src/$moduleName/bin"
        Copy-Item -Path "$platformDirectory/*" -Destination "$tempFolder/src/$moduleName/bin" -Recurse -Force

        Write-Host "Copying platform script files from $platformSourcePath to $tempFolder/src/$moduleName"
        Copy-Item -Path "$platformSourcePath/__init__.py" -Destination "$tempFolder/src/$moduleName/__init__.py" -Force

        # Remove symbols files before packing
        Write-Host "Removing symbol files from $tempFolder"
        Get-ChildItem -Path $tempFolder -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        # Read template and replace placeholders
        $pyprojectTemplate = Get-Content "$platformSourcePath/pyproject.toml.template" -Raw
        $osClassifier = $osClassifierMap[$pypiOs]

        $pyprojectContent = $pyprojectTemplate `
            -replace '{{PACKAGE_NAME}}', $platformPackageName `
            -replace '{{VERSION}}', $server.version `
            -replace '{{DESCRIPTION}}', "$description, for $pypiOs on $arch" `
            -replace '{{KEYWORDS}}', (Get-KeywordsString $keywords) `
            -replace '{{OS_CLASSIFIER}}', $osClassifier `
            -replace '{{MODULE_NAME}}', $moduleName `
            -replace '{{HOMEPAGE}}', $server.readmeUrl

        $pyprojectPath = "$tempFolder/pyproject.toml"
        Write-Host "Writing $pyprojectPath"
        $pyprojectContent | Out-File -FilePath $pyprojectPath -Encoding utf8 -Force

        # Update version in __init__.py
        $initPyPath = "$tempFolder/src/$moduleName/__init__.py"
        $initPyContent = Get-Content $initPyPath -Raw
        $initPyContent = $initPyContent -replace '__version__ = "0\.0\.0"', "__version__ = `"$($server.version)`""
        $initPyContent | Out-File -FilePath $initPyPath -Encoding utf8 -Force

        # Set executable permissions on non-Windows
        if (!$IsWindows) {
            Write-Host "Setting executable permissions for $tempFolder/src/$moduleName/$binPath" -ForegroundColor Yellow
            $binFullPath = "$tempFolder/src/$moduleName/$binPath"
            if (Test-Path $binFullPath) {
                Invoke-LoggedCommand "chmod +x `"$binFullPath`""
            }
        }
        else {
            Write-Warning "Executable permissions are not set when packing on a Windows agent."
        }

        # Process and copy README
        & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
            -Command "extract" `
            -InputReadMePath "$RepoRoot/$($server.readmePath)" `
            -PackageType "pypi" `
            -InsertPayload @{ ToolTitle = 'PyPI Package' } `
            -OutputDirectory $tempFolder

        Write-Host "Copying LICENSE and NOTICE.txt to $tempFolder"
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force

        # Build the wheel and sdist
        Write-Host "Building PyPI package for $platformPackageName" -ForegroundColor Green
        Push-Location $tempFolder
        try {
            # Check if python or python3 is available
            $pythonCmd = if (Get-Command python3 -ErrorAction SilentlyContinue) { "python3" } else { "python" }
            
            Invoke-LoggedCommand "$pythonCmd -m pip install --quiet build"
            Invoke-LoggedCommand "$pythonCmd -m build --wheel --sdist"

            # Copy the built packages to output
            $distPath = "$tempFolder/dist"
            if (Test-Path $distPath) {
                $platformPackageOutputPath = "$platformOutputPath/$platformPackageName"
                New-Item -ItemType Directory -Force -Path $platformPackageOutputPath | Out-Null
                Copy-Item -Path "$distPath/*" -Destination $platformPackageOutputPath -Force
                Write-Host "Package created at $platformPackageOutputPath" -ForegroundColor Green
            }
        }
        finally {
            Pop-Location
        }

        # Track for wrapper package
        $platformKey = "$pypiOs-$arch"
        $platformDependencies[$platformKey] = "$platformPackageName==$($server.version)"
        $supportedPlatforms += [ordered]@{
            os   = $pypiOs
            arch = $arch
            name = $platformPackageName
        }
    }

    # Now build the wrapper package
    Write-Host "`nBuilding wrapper package $basePackageName" -ForegroundColor Cyan

    Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $tempFolder | Out-Null

    $wrapperModuleName = Get-ModuleName $basePackageName
    New-Item -ItemType Directory -Force -Path "$tempFolder/src/$wrapperModuleName" | Out-Null

    # Copy wrapper __init__.py
    Copy-Item -Path "$wrapperSourcePath/__init__.py" -Destination "$tempFolder/src/$wrapperModuleName/__init__.py" -Force

    # Update version in __init__.py
    $initPyPath = "$tempFolder/src/$wrapperModuleName/__init__.py"
    $initPyContent = Get-Content $initPyPath -Raw
    $initPyContent = $initPyContent -replace '__version__ = "0\.0\.0"', "__version__ = `"$($server.version)`""
    $initPyContent | Out-File -FilePath $initPyPath -Encoding utf8 -Force

    # Generate pyproject.toml for wrapper
    $wrapperPyprojectTemplate = Get-Content "$wrapperSourcePath/pyproject.toml.template" -Raw

    $wrapperPyprojectContent = $wrapperPyprojectTemplate `
        -replace '{{PACKAGE_NAME}}', $basePackageName `
        -replace '{{VERSION}}', $server.version `
        -replace '{{DESCRIPTION}}', $description `
        -replace '{{KEYWORDS}}', (Get-KeywordsString $keywords) `
        -replace '{{CLI_NAME}}', $cliName `
        -replace '{{MODULE_NAME}}', $wrapperModuleName `
        -replace '{{HOMEPAGE}}', $server.readmeUrl

    $pyprojectPath = "$tempFolder/pyproject.toml"
    Write-Host "Writing $pyprojectPath"
    $wrapperPyprojectContent | Out-File -FilePath $pyprojectPath -Encoding utf8 -Force

    # Process and copy README
    & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
        -Command "extract" `
        -InputReadMePath "$RepoRoot/$($server.readmePath)" `
        -PackageType "pypi" `
        -InsertPayload @{ ToolTitle = 'PyPI Package' } `
        -OutputDirectory $tempFolder

    Write-Host "Copying LICENSE and NOTICE.txt to $tempFolder"
    Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
    Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force

    # Build the wrapper wheel and sdist
    Write-Host "Building PyPI wrapper package for $basePackageName" -ForegroundColor Green
    Push-Location $tempFolder
    try {
        $pythonCmd = if (Get-Command python3 -ErrorAction SilentlyContinue) { "python3" } else { "python" }
        
        Invoke-LoggedCommand "$pythonCmd -m pip install --quiet build"
        Invoke-LoggedCommand "$pythonCmd -m build --wheel --sdist"

        # Copy the built packages to output
        $distPath = "$tempFolder/dist"
        if (Test-Path $distPath) {
            $wrapperPackageOutputPath = "$wrapperOutputPath/$basePackageName"
            New-Item -ItemType Directory -Force -Path $wrapperPackageOutputPath | Out-Null
            Copy-Item -Path "$distPath/*" -Destination $wrapperPackageOutputPath -Force
            Write-Host "Wrapper package created at $wrapperPackageOutputPath" -ForegroundColor Green
        }
    }
    finally {
        Pop-Location
    }

    Write-Host "`n✅ PyPI packages built successfully for $($server.name)" -ForegroundColor Green
    Write-Host "   Wrapper: $basePackageName"
    Write-Host "   Platforms: $($supportedPlatforms.name -join ', ')"
}

# Main execution
foreach ($server in $buildInfo.servers) {
    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Building PyPI packages for $($server.name)" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    # Build non-native packages
    BuildServerPackages $server $false

    # Build native packages if available
    $hasNative = ($server.platforms | Where-Object { $_.native -eq $true }).Count -gt 0
    if ($hasNative) {
        BuildServerPackages $server $true
    }
}

# Cleanup temp folder
Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "PyPI packaging complete!" -ForegroundColor Green
Write-Host "Output: $OutputPath" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

exit $exitCode
