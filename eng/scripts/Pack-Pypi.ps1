#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Packs Azure MCP Server binaries into PyPI wheels for distribution.

.DESCRIPTION
    This script creates platform-specific PyPI wheels for the Azure MCP Server.
    Each wheel contains the binary for a specific platform (OS + architecture).
    
    The wheels are named according to PyPI conventions:
    - msmcp_azure-1.0.0-py3-none-win_amd64.whl
    - msmcp_azure-1.0.0-py3-none-macosx_11_0_arm64.whl
    - msmcp_azure-1.0.0-py3-none-manylinux_2_17_x86_64.whl
    etc.

    Users can install with:
    - pip install msmcp-azure
    - uvx msmcp-azure
    - pipx install msmcp-azure

.PARAMETER ArtifactsPath
    Path to the build artifacts containing the server binaries.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and platform details.

.PARAMETER OutputPath
    Path where the PyPI packages will be created.

.EXAMPLE
    ./Pack-Pypi.ps1
    Creates PyPI packages using default local paths.

.EXAMPLE
    ./Pack-Pypi.ps1 -ArtifactsPath ".work/build" -BuildInfoPath ".work/build_info.json"
    Creates PyPI packages using specified artifact and build info paths.
#>

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"
$RepoRoot = $RepoRoot.Path.Replace('\', '/')

$pypiSourcePath = "$RepoRoot/eng/pypi"

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

# Map node OS names to PyPI wheel platform tags
# See: https://packaging.python.org/en/latest/specifications/platform-compatibility-tags/
$wheelPlatformMap = @{
    'win32-x64'      = 'win_amd64'
    'win32-arm64'    = 'win_arm64'
    'darwin-x64'     = 'macosx_11_0_x86_64'
    'darwin-arm64'   = 'macosx_11_0_arm64'
    'linux-x64'      = 'manylinux_2_17_x86_64.manylinux2014_x86_64'
    'linux-arm64'    = 'manylinux_2_17_aarch64.manylinux2014_aarch64'
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

function Get-PythonCommand {
    # Try python3 first (common on Linux/macOS), but verify it works
    # On Windows, "python3" may be a Store alias that doesn't work
    try {
        $null = & python3 --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            return "python3"
        }
    } catch {}
    
    # Fall back to python
    try {
        $null = & python --version 2>&1
        if ($LASTEXITCODE -eq 0) {
            return "python"
        }
    } catch {}
    
    throw "Python is not installed or not in PATH. Please install Python 3.10+."
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
    New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null

    # Use PyPI package name from csproj, skip servers without PyPI configuration
    $basePackageName = if ([string]::IsNullOrWhiteSpace($server.pypiPackageName)) { $null } else { $server.pypiPackageName }
    if (!$basePackageName) {
        Write-Host "Skipping $($server.name) - no PyPI package name configured" -ForegroundColor Yellow
        return
    }
    
    $description = if ([string]::IsNullOrWhiteSpace($server.pypiDescription)) { $server.description } else { $server.pypiDescription }
    $cliName = $server.cliName
    $keywords = @(if ($server.pypiPackageKeywords) { $server.pypiPackageKeywords } else { $server.npmPackageKeywords })
    $moduleName = Get-ModuleName $basePackageName

    if ($native) {
        $basePackageName += "-native"
        $description += " with native dependencies"
        $keywords += "native"
        $moduleName = Get-ModuleName $basePackageName
    }

    $builtPlatforms = @()

    # Build a wheel for each platform
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

        $pypiOs = $platform.nodeOs
        $arch = $platform.architecture
        $platformKey = "$pypiOs-$arch"
        $wheelPlatformTag = $wheelPlatformMap[$platformKey]

        if (!$wheelPlatformTag) {
            Write-Warning "Unknown platform: $platformKey, skipping"
            continue
        }

        $osClassifier = $osClassifierMap[$pypiOs]
        $extension = $platform.extension

        Write-Host "`nBuilding wheel for $basePackageName ($platformKey)" -ForegroundColor Cyan

        # Clean temp folder
        Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $tempFolder | Out-Null
        New-Item -ItemType Directory -Force -Path "$tempFolder/src/$moduleName/bin" | Out-Null

        # Copy binary files
        Write-Host "  Copying binaries from $platformDirectory"
        Copy-Item -Path "$platformDirectory/*" -Destination "$tempFolder/src/$moduleName/bin" -Recurse -Force

        # Copy package __init__.py
        Copy-Item -Path "$pypiSourcePath/__init__.py" -Destination "$tempFolder/src/$moduleName/__init__.py" -Force

        # Remove symbols files before packing
        Write-Host "  Removing symbol files"
        Get-ChildItem -Path $tempFolder -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        # Read template and replace placeholders
        $pyprojectTemplate = Get-Content "$pypiSourcePath/pyproject.toml.template" -Raw

        $pyprojectContent = $pyprojectTemplate `
            -replace '{{PACKAGE_NAME}}', $basePackageName `
            -replace '{{VERSION}}', $server.version `
            -replace '{{DESCRIPTION}}', $description `
            -replace '{{KEYWORDS}}', (Get-KeywordsString $keywords) `
            -replace '{{OS_CLASSIFIER}}', $osClassifier `
            -replace '{{CLI_NAME}}', $cliName `
            -replace '{{MODULE_NAME}}', $moduleName `
            -replace '{{HOMEPAGE}}', $server.readmeUrl

        $pyprojectPath = "$tempFolder/pyproject.toml"
        Write-Host "  Writing pyproject.toml"
        $pyprojectContent | Out-File -FilePath $pyprojectPath -Encoding utf8 -Force

        # Update version in __init__.py
        $initPyPath = "$tempFolder/src/$moduleName/__init__.py"
        $initPyContent = Get-Content $initPyPath -Raw
        $initPyContent = $initPyContent -replace '__version__ = "0\.0\.0"', "__version__ = `"$($server.version)`""
        $initPyContent | Out-File -FilePath $initPyPath -Encoding utf8 -Force

        # Set executable permissions on non-Windows
        $binPath = "bin/$cliName$extension"
        if (!$IsWindows) {
            Write-Host "  Setting executable permissions" -ForegroundColor Yellow
            $binFullPath = "$tempFolder/src/$moduleName/$binPath"
            if (Test-Path $binFullPath) {
                Invoke-LoggedCommand "chmod +x `"$binFullPath`""
            }
        }
        else {
            Write-Warning "  Executable permissions are not set when packing on a Windows agent."
        }

        # Process and copy README
        $insertPayload = @{
            ToolTitle = 'PyPI Package'
            MCPRepositoryMetadata = "<!-- mcp-name: $($server.mcpRepositoryName) -->"
        }

        & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
            -Command "extract" `
            -InputReadMePath "$RepoRoot/$($server.readmePath)" `
            -PackageType "pypi" `
            -InsertPayload $insertPayload `
            -OutputDirectory $tempFolder

        Write-Host "  Copying LICENSE and NOTICE.txt"
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force

        # Build the wheel with platform-specific tag
        Write-Host "  Building wheel" -ForegroundColor Green
        Push-Location $tempFolder
        try {
            $pythonCmd = Get-PythonCommand
            
            Invoke-LoggedCommand "$pythonCmd -m pip install --quiet build==1.2.2 wheel==0.45.1"
            
            # Build wheel only (no sdist for platform packages)
            # We use --wheel and then rename to set the correct platform tag
            Invoke-LoggedCommand "$pythonCmd -m build --wheel"

            # Rename the wheel to include the correct platform tag
            $distPath = "$tempFolder/dist"
            if (Test-Path $distPath) {
                $wheels = Get-ChildItem -Path $distPath -Filter "*.whl"
                foreach ($wheel in $wheels) {
                    # The default wheel name is like: msmcp_azure-1.0.0-py3-none-any.whl
                    # We need to change it to: msmcp_azure-1.0.0-py3-none-<platform>.whl
                    $newName = $wheel.Name -replace '-py3-none-any\.whl$', "-py3-none-$wheelPlatformTag.whl"
                    $newPath = Join-Path $distPath $newName
                    
                    Write-Host "  Renaming wheel to $newName"
                    Move-Item -Path $wheel.FullName -Destination $newPath -Force
                    
                    # Copy to output
                    Copy-Item -Path $newPath -Destination $serverOutputPath -Force
                    Write-Host "  ✅ Created: $newName" -ForegroundColor Green
                }
            }
        }
        finally {
            Pop-Location
        }

        $builtPlatforms += $platformKey
    }

    Write-Host "`n✅ PyPI packages built successfully for $($server.name)" -ForegroundColor Green
    Write-Host "   Package: $basePackageName"
    Write-Host "   Platforms: $($builtPlatforms -join ', ')"
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
