#!/bin/env pwsh
#Requires -Version 7

<#
.SYNOPSIS
    Creates MCPB (MCP Bundle) packages from trimmed server binaries.

.DESCRIPTION
    This script packages MCP servers into the MCPB format using the official MCPB CLI tool.
    It reads server and platform information from build_info.json and creates unsigned .mcpb
    files for each platform. The packages can then be signed using ESRP in a separate step.

.PARAMETER ArtifactsPath
    Path to the build artifacts directory containing the trimmed server binaries.
    Defaults to ".work/build" in the repo root.

.PARAMETER BuildInfoPath
    Path to the build_info.json file containing server and platform details.
    Defaults to ".work/build_info.json" in the repo root.

.PARAMETER OutputPath
    Output directory for the .mcpb files.
    Defaults to ".work/packages_mcpb" in the repo root.

.PARAMETER KeepStagingDirectory
    If specified, keeps the staging directory after packaging for debugging purposes.
    The staging directory is located at ".work/temp_mcpb" in the repo root.

.EXAMPLE
    ./Pack-Mcpb.ps1

.EXAMPLE
    ./Pack-Mcpb.ps1 -ArtifactsPath ".work/build" -BuildInfoPath ".work/build_info.json" -OutputPath ".work/packages_mcpb"
#>

[CmdletBinding()]
param(
    [string] $ArtifactsPath,
    [string] $BuildInfoPath,
    [string] $OutputPath,
    [switch] $KeepStagingDirectory
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/../common/scripts/common.ps1"

$RepoRoot = $RepoRoot.Path.Replace('\', '/')
$ignoreMissingArtifacts = $env:TF_BUILD -ne 'true'
$exitCode = 0

if (!$ArtifactsPath) {
    $ArtifactsPath = "$RepoRoot/.work/build"
}

if (!$BuildInfoPath) {
    $BuildInfoPath = "$RepoRoot/.work/build_info.json"
}

if (!$OutputPath) {
    $OutputPath = "$RepoRoot/.work/packages_mcpb"
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

# Ensure MCPB CLI is installed
if (-not (Get-Command mcpb -ErrorAction SilentlyContinue)) {
    Write-Host "Installing MCPB CLI..."
    Invoke-LoggedCommand 'dotnet tool install --global Mcpb.Cli'
}

$buildInfo = Get-Content $BuildInfoPath -Raw | ConvertFrom-Json

if (Test-Path $OutputPath) {
    Write-Host "Cleaning existing output path $OutputPath"
    Remove-Item -Path $OutputPath -Recurse -Force -ErrorAction SilentlyContinue
}

New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

$tempPath = "$RepoRoot/.work/temp_mcpb"

foreach ($server in $buildInfo.servers) {
    $mcpbDirectory = "$RepoRoot/servers/$($server.name)/mcpb"
    $manifestPath = "$mcpbDirectory/manifest.json"

    if (!(Test-Path $manifestPath)) {
        LogWarning "MCPB manifest not found at $manifestPath. Skipping server $($server.name)."
        continue
    }

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Packing MCPB for server: $($server.name)" -ForegroundColor Cyan
    Write-Host "Version: $($server.version)" -ForegroundColor Cyan
    Write-Host "========================================`n" -ForegroundColor Cyan

    # Filter platforms: only trimmed, non-native, non-special-purpose platforms
    $filteredPlatforms = $server.platforms | Where-Object { 
        $_.trimmed -eq $true -and 
        -not $_.native -and 
        -not $_.specialPurpose 
    }

    if ($filteredPlatforms.Count -eq 0) {
        Write-Host "No suitable platforms found for MCPB packaging for server $($server.name)"
        continue
    }

    foreach ($platform in $filteredPlatforms) {
        $platformDirectory = "$ArtifactsPath/$($platform.artifactPath)"
        $platformName = $platform.name

        if (!(Test-Path $platformDirectory)) {
            $message = "Platform directory $platformDirectory does not exist."
            if ($ignoreMissingArtifacts) {
                LogWarning $message
                continue
            } else {
                LogError $message
                $exitCode = 1
                continue
            }
        }

        Write-Host @"

------------------------------------------------------------------------------------
Processing MCPB packaging:
  Server: $($server.name)
  Platform: $platformName
  Version: $($server.version)
  CLI Name: $($server.cliName)
  Binaries Path: $platformDirectory
------------------------------------------------------------------------------------

"@

        # Create clean staging directory for this platform
        $stagingDir = "$tempPath/$($server.name)/$platformName"
        Remove-Item -Path $stagingDir -Recurse -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $stagingDir | Out-Null
        New-Item -ItemType Directory -Force -Path "$stagingDir/server" | Out-Null

        # Copy trimmed binaries to server/ subdirectory
        Write-Host "Copying trimmed binaries from $platformDirectory..."
        Copy-Item -Path "$platformDirectory/*" -Destination "$stagingDir/server" -Recurse

        # Copy and update manifest.json for this platform
        # The source manifest uses platform-agnostic paths (no extension), so we need to add
        # the correct extension for this platform's executable
        Write-Host "Copying manifest from $manifestPath..."
        $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
        
        $executableName = $server.cliName + $platform.extension
        
        # Update entry_point with platform-specific executable (relative path for mcpb pack verification)
        $manifest.server.entry_point = "server/$executableName"
        
        # Update command with platform-specific executable (keeps ${__dirname} for runtime)
        $manifest.server.mcp_config.command = "`${__dirname}/server/$executableName"
        
        # Remove platform_overrides since this bundle is platform-specific
        if ($manifest.server.mcp_config.PSObject.Properties.Name -contains 'platform_overrides') {
            $manifest.server.mcp_config.PSObject.Properties.Remove('platform_overrides')
        }
        
        $manifest | ConvertTo-Json -Depth 100 | Set-Content "$stagingDir/manifest.json" -NoNewline

        # Copy and rename icon to servericon.png (required name for MCPB bundles)
        $iconCandidates = @("servericon.png", "azureicon.png", "icon.png")
        $iconFound = $false
        foreach ($candidate in $iconCandidates) {
            $candidatePath = "$mcpbDirectory/$candidate"
            if (Test-Path $candidatePath) {
                Write-Host "Copying icon from $candidatePath..."
                Copy-Item $candidatePath "$stagingDir/servericon.png"
                $iconFound = $true
                break
            }
        }

        # Also check the server's packageIcon from build info
        if (-not $iconFound -and $server.packageIcon) {
            $packageIconPath = "$RepoRoot/$($server.packageIcon)"
            if (Test-Path $packageIconPath) {
                Write-Host "Copying icon from $packageIconPath..."
                Copy-Item $packageIconPath "$stagingDir/servericon.png"
                $iconFound = $true
            }
        }

        if (-not $iconFound) {
            LogWarning "No icon found for $($server.name). MCPB may not validate."
        }

        # Copy LICENSE and NOTICE.txt
        Copy-Item -Path "$RepoRoot/LICENSE" -Destination $stagingDir -Force
        Copy-Item -Path "$RepoRoot/NOTICE.txt" -Destination $stagingDir -Force

        # Validate manifest
        Write-Host "Validating manifest..."
        $validateResult = mcpb validate $stagingDir 2>&1
        if ($LASTEXITCODE -ne 0) {
            LogError "Manifest validation failed for $($server.name) on $platformName"
            LogError $validateResult
            $exitCode = 1
            continue
        }
        Write-Host "Manifest validation passed" -ForegroundColor Green

        # Create output directory for this server/platform
        $serverOutputPath = "$OutputPath/$($server.artifactPath)"
        New-Item -ItemType Directory -Force -Path $serverOutputPath | Out-Null

        # Pack the MCPB
        # Use --update to auto-update manifest with discovered tools from the server
        $mcpbFileName = "$($server.name)-$($platform.dotnetOs)-$($platform.architecture).mcpb"
        $mcpbFilePath = "$serverOutputPath/$mcpbFileName"
        
        Write-Host "Packing MCPB to $mcpbFilePath..."
        & mcpb pack $stagingDir $mcpbFilePath --update
        if ($LASTEXITCODE -ne 0) {
            LogError "MCPB packing failed for $($server.name) on $platformName"
            $exitCode = 1
            continue
        }

        # Get file size for reporting
        $fileSize = (Get-Item $mcpbFilePath).Length
        $fileSizeMB = [math]::Round($fileSize / 1MB, 2)

        Write-Host "Created: $mcpbFilePath ($fileSizeMB MB)" -ForegroundColor Green

        # Show package info
        Write-Host "`nPackage info:"
        mcpb info $mcpbFilePath
    }
}

# Cleanup temp directory (unless -KeepStagingDirectory is specified)
if ($KeepStagingDirectory) {
    Write-Host "`nStaging directory preserved at: $tempPath" -ForegroundColor Yellow
} else {
    Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue
}

if ($exitCode -eq 0) {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "MCPB packaging completed successfully!" -ForegroundColor Green
    Write-Host "Output: $OutputPath" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green
} else {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "MCPB packaging completed with errors." -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Red
}

exit $exitCode
