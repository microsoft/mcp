#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Packs MCP servers into PyPI wheels for distribution.

.DESCRIPTION
    Azure MCP uses a universal Python launcher wheel with an integrity manifest
    for version-pinned GitHub Release ZIPs. Other servers retain bundled binaries.
    
    Azure wheels use the universal tag, e.g. msmcp_azure-1.0.0-py3-none-any.whl.
    Other servers retain OS/architecture tags such as win_amd64.

    Users can install with:
    - pip install msmcp-azure
    - uvx msmcp-azure
    - pipx install msmcp-azure

.PARAMETER ArtifactsPath
    Path to the build artifacts containing the server binaries.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and platform details.

.PARAMETER ReleaseArtifactsPath
    Path to the exact signed ZIPs uploaded to GitHub Releases (required for Azure).

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
    [string] $OutputPath,
    [string] $ReleaseArtifactsPath
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

if (!$ReleaseArtifactsPath) {
    $ReleaseArtifactsPath = "$RepoRoot/.work/binaries_signed_zipped"
}

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_pypi"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
}

if (!(Test-Path $BuildInfoPath)) {
    LogError "Build info file $BuildInfoPath does not exist. Run eng/scripts/New-BuildInfo.ps1 to create it."
    $exitCode = 1
}

if ($exitCode -ne 0) {
    exit $exitCode
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json -AsHashtable

if (($buildInfo.servers | Where-Object { $_.name -ne 'Azure.Mcp.Server' }) -and !(Test-Path $ArtifactsPath)) {
    LogError "Artifacts path $ArtifactsPath does not exist."
    exit 1
}

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

function ConvertTo-PythonPackageVersion {
    param ([string] $Version)

    $parsedVersion = [AzureEngSemanticVersion]::ParseVersionString($Version)
    if (!$parsedVersion) {
        return $null
    }

    # Discard the returned regex: PowerShell otherwise emits it alongside the version,
    # making this function return an array whose ToString() produces "System.Object[]".
    $null = $parsedVersion.SetupPythonConventions()
    if ($parsedVersion.PrereleaseLabel -eq 'beta') {
        $parsedVersion.PrereleaseLabel = 'b'
    }
    elseif ($parsedVersion.PrereleaseLabel -eq 'alpha') {
        $parsedVersion.PrereleaseLabel = 'a'
    }

    return $parsedVersion.ToString()
}

function Get-DevStatus {
    param ([string] $version)

    $parsedVersion = [AzureEngSemanticVersion]::ParseVersionString($version)
    if (!$parsedVersion) {
        return $null
    }

    if (!$parsedVersion.IsPrerelease) {
        return '5 - Production/Stable'
    }

    # The parser treats unlabeled major-zero versions as prerelease beta versions.
    if ($parsedVersion.Major -eq 0 -and [string]::IsNullOrEmpty($parsedVersion.PrereleaseLabel)) {
        return '4 - Beta'
    }

    switch ($parsedVersion.PrereleaseLabel) {
        'alpha' { return '3 - Alpha' }
        'beta'  { return '4 - Beta' }
        'rc'    { return '4 - Beta' }
        default { throw "Unknown prerelease label '$($parsedVersion.PrereleaseLabel)' for version '$version'." }
    }
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

function New-ReleaseManifest([hashtable] $server, [object[]] $platforms, [bool] $native) {
    if ([string]::IsNullOrWhiteSpace($server.releaseTag)) {
        throw "Missing release tag for $($server.name)."
    }
    $manifest = [ordered]@{
        version = $server.version
        platforms = [ordered]@{}
    }
    foreach ($platform in $platforms) {
        $key = "$($platform.nodeOs)-$($platform.architecture)"
        if (!$wheelPlatformMap.ContainsKey($key)) {
            throw "Unsupported Azure PyPI release platform: $key"
        }
        $suffix = if ($native) { '-native' } else { '' }
        $asset = "$($server.name)-$($platform.dotnetOs)-$($platform.architecture)$suffix.zip"
        $archivePath = Join-Path $ReleaseArtifactsPath $asset
        if (!(Test-Path -LiteralPath $archivePath -PathType Leaf)) {
            # Never publish a launcher that silently lacks a configured release platform.
            throw "Required signed release archive does not exist: $archivePath"
        }
        $files = [ordered]@{}
        $archive = [System.IO.Compression.ZipFile]::OpenRead($archivePath)
        try {
            foreach ($entry in $archive.Entries) {
                if ($entry.FullName.EndsWith('/')) {
                    continue
                }
                $stream = $entry.Open()
                try {
                    $hash = [System.Security.Cryptography.SHA256]::HashData($stream)
                    $files[$entry.FullName] = @{
                        sha256 = [Convert]::ToHexString($hash).ToLowerInvariant()
                        size = $entry.Length
                    }
                }
                finally {
                    $stream.Dispose()
                }
            }
        }
        finally {
            $archive.Dispose()
        }
        $executable = "$($server.cliName)$($platform.extension)"
        if (!$files.Contains($executable)) {
            throw "Release archive $asset does not contain $executable."
        }
        $manifest.platforms[$key] = @{
            url = "https://github.com/microsoft/mcp/releases/download/$($server.releaseTag)/$asset"
            sha256 = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash.ToLowerInvariant()
            size = (Get-Item -LiteralPath $archivePath).Length
            files = $files
        }
    }
    return $manifest
}

function BuildServerPackages([hashtable] $server, [bool] $native) {
    $isLauncher = $server.name -eq 'Azure.Mcp.Server'
    $serverDirectory = "$ArtifactsPath/$($server.artifactPath)"

    if (!$isLauncher -and !(Test-Path $serverDirectory)) {
        $message = "Server directory $serverDirectory does not exist."
        if ($ignoreMissingArtifacts) {
            Write-Warning $message
        }
        else {
            Write-Error $message
        }
        return
    }

    $filteredPlatforms = @($server.platforms | Where-Object {
        $_.native -eq $native -and (
            -not $_.specialPurpose -or ($isLauncher -and $_.specialPurpose -eq 'native')
        )
    })
    if ($filteredPlatforms.Count -eq 0) {
        Write-Host "No platforms to build for server $($server.name) with native=$native"
        return
    }

    $serverOutputPath = "$OutputPath/$($server.artifactPath)"
    if ($isLauncher -and $native) {
        $serverOutputPath += "-native"
    }
    New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null

    # Use PyPI package name from csproj, skip servers without PyPI configuration
    $basePackageName = if ([string]::IsNullOrWhiteSpace($server.pypiPackageName)) { $null } else { $server.pypiPackageName }
    if (!$basePackageName) {
        Write-Host "Skipping $($server.name) - no PyPI package name configured" -ForegroundColor Yellow
        return
    }

    $pythonVersion = ConvertTo-PythonPackageVersion $server.version
    if (!$pythonVersion) {
        Write-Error "Server version '$($server.version)' is not a valid semantic version for PyPI packaging."
        return
    }
    $pythonVersion = $pythonVersion.ToString()
    $devStatus = Get-DevStatus $server.version
    
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

    # A launcher wheel covers every platform in its manifest; bundled servers keep one wheel per platform.
    $packagePlatforms = $filteredPlatforms
    if ($isLauncher) {
        $releaseManifest = New-ReleaseManifest $server $filteredPlatforms $native
        $packagePlatforms = @($filteredPlatforms[0])
    }
    foreach ($platform in $packagePlatforms) {
        $platformDirectory = "$ArtifactsPath/$($platform.artifactPath)"

        if (!$isLauncher -and !(Test-Path $platformDirectory)) {
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

        $osClassifier = if ($isLauncher) { 'OS Independent' } else { $osClassifierMap[$pypiOs] }
        $extension = $platform.extension

        Write-Host "`nBuilding wheel for $basePackageName ($platformKey)" -ForegroundColor Cyan

        # Clean temp folder
        Remove-Item -Path $tempFolder -Recurse -Force -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $tempFolder | Out-Null
        New-Item -ItemType Directory -Force -Path "$tempFolder/src/$moduleName" | Out-Null
        if ($isLauncher) {
            Copy-Item -Path "$pypiSourcePath/azure_launcher.py" -Destination "$tempFolder/src/$moduleName/__init__.py"
            $releaseManifest | ConvertTo-Json -Depth 10 |
                Set-Content -LiteralPath "$tempFolder/src/$moduleName/release.json" -Encoding utf8NoBOM
        }
        else {
            New-Item -ItemType Directory -Force -Path "$tempFolder/src/$moduleName/bin" | Out-Null
            Write-Host "  Copying binaries from $platformDirectory"
            Copy-Item -Path "$platformDirectory/*" -Destination "$tempFolder/src/$moduleName/bin" -Recurse -Force
            Copy-Item -Path "$pypiSourcePath/__init__.py" -Destination "$tempFolder/src/$moduleName/__init__.py" -Force
        }

        # Remove symbols files before packing
        Write-Host "  Removing symbol files"
        Get-ChildItem -Path $tempFolder -Recurse -Include "*.pdb", "*.dSYM", "*.dbg" | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -ProgressAction SilentlyContinue

        # Read template and replace placeholders
        $pyprojectTemplate = Get-Content "$pypiSourcePath/pyproject.toml.template" -Raw

        $pyprojectContent = $pyprojectTemplate `
            -replace '{{PACKAGE_NAME}}', $basePackageName `
            -replace '{{VERSION}}', $pythonVersion `
            -replace '{{DESCRIPTION}}', $description `
            -replace '{{KEYWORDS}}', (Get-KeywordsString $keywords) `
            -replace '{{OS_CLASSIFIER}}', $osClassifier `
            -replace '{{CLI_NAME}}', $cliName `
            -replace '{{MODULE_NAME}}', $moduleName `
            -replace '{{HOMEPAGE}}', $server.readmeUrl `
            -replace '{{DEV_STATUS}}', $devStatus

        $pyprojectPath = "$tempFolder/pyproject.toml"
        Write-Host "  Writing pyproject.toml"
        Write-Host $pyprojectContent
        $pyprojectContent | Out-File -FilePath $pyprojectPath -Encoding utf8 -Force

        # Configure package identity and CLI name in __init__.py.
        $initPyPath = "$tempFolder/src/$moduleName/__init__.py"
        $initPyContent = Get-Content $initPyPath -Raw
        $initPyContent = $initPyContent `
            -replace '{{VERSION}}', $pythonVersion `
            -replace '{{PACKAGE_NAME}}', $basePackageName `
            -replace '{{CLI_NAME}}', $cliName
        $initPyContent | Out-File -FilePath $initPyPath -Encoding utf8 -Force

        # Set executable permissions on non-Windows
        $binPath = "bin/$cliName$extension"
        if ($isLauncher) {
            Write-Host "  Executable permissions will be set by the launcher after download."
        }
        elseif (!$IsWindows) {
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
            # The mcp-name HTML comment is required by the MCP registry for package ownership validation.
            # It must appear as 'mcp-name: <server-name>' in the package README or publishing will fail with a 400 error.
            MCPRepositoryMetadata = "<!-- mcp-name: $($server.mcpRepositoryName) -->"
        }

        & "$RepoRoot/eng/scripts/Process-PackageReadMe.ps1" `
            -Command "extract" `
            -InputReadMePath "$RepoRoot/$($server.readmePath)" `
            -PackageType "pypi" `
            -InsertPayload $insertPayload `
            -OutputDirectory $tempFolder

        if ($isLauncher) {
            @'

## Executable download and cache

This package contains a small Python launcher, not the Azure MCP executable.
The first invocation downloads the matching OS/architecture ZIP from the
version-pinned `microsoft/mcp` GitHub Release over HTTPS. SHA-256 hashes embedded
in this package verify the archive and all cached files before execution.
Subsequent invocations reuse the cache and work offline.

The cache defaults to the user's local application cache directory. Set
`AZURE_MCP_CACHE_DIR` to choose a writable cache location or to share a cache
between Python environments. To provision an offline environment, run the
same package version and platform once with network access, then copy that
cache to the offline host. An empty or corrupt cache requires network access.
Downloads require access to `github.com` and GitHub's release asset hosts.
Download diagnostics go to stderr; MCP stdout remains unchanged.
'@ | Add-Content -LiteralPath "$tempFolder/README.md"
        }

        Write-Host "  Copying LICENSE and NOTICE.txt"
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $tempFolder -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $tempFolder -Force

        # Azure keeps the universal wheel tag; other servers retain platform-specific tags.
        Write-Host "  Building wheel" -ForegroundColor Green
        Push-Location $tempFolder
        try {
            $pythonCmd = Get-PythonCommand
            
            Invoke-LoggedCommand "$pythonCmd -m pip install --quiet build==1.2.2 wheel==0.45.1"
            
            # Build wheels only; bundled servers are renamed to their platform tag below.
            Invoke-LoggedCommand "$pythonCmd -m build --wheel"

            # Rename the wheel to include the correct platform tag
            $distPath = "$tempFolder/dist"
            if (Test-Path $distPath) {
                $wheels = Get-ChildItem -Path $distPath -Filter "*.whl"
                foreach ($wheel in $wheels) {
                    $newName = if ($isLauncher) { $wheel.Name } else {
                        $wheel.Name -replace '-py3-none-any\.whl$', "-py3-none-$wheelPlatformTag.whl"
                    }
                    $newPath = Join-Path $distPath $newName
                    
                    Write-Host "  Renaming wheel to $newName"
                    if ($newName -ne $wheel.Name) {
                        Move-Item -Path $wheel.FullName -Destination $newPath -Force
                    }
                    
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
