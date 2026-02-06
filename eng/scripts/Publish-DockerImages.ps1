<#
.SYNOPSIS
    Publishes multi-architecture Docker images to a container registry.

.DESCRIPTION
    This script loads Docker images from tar files, tags them with architecture suffixes,
    pushes them to a container registry, and creates multi-arch manifests.

.PARAMETER CliName
    The CLI name used to identify tar files (e.g., 'azmcp' matches 'azmcp-amd64-image.tar').

.PARAMETER Version
    The version tag for the Docker images (e.g., '2.0.0').

.PARAMETER BaseRepo
    The base repository URL without tag (e.g., 'azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp').

.PARAMETER TarDirectory
    The directory containing the Docker image tar files.

.EXAMPLE
    ./Publish-DockerImages.ps1 -CliName 'azmcp' -Version '2.0.0' -BaseRepo 'azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp' -TarDirectory './docker_output'
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$CliName,

    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$BaseRepo,

    [Parameter(Mandatory = $true)]
    [string]$TarDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-DockerCommand {
    param(
        [string[]]$Arguments,
        [switch]$CaptureOutput
    )
    
    Write-Host "docker $($Arguments -join ' ')" -ForegroundColor DarkGray
    
    if ($CaptureOutput) {
        $output = & docker @Arguments 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker command failed with exit code $LASTEXITCODE`: $output"
        }
        return $output
    }
    else {
        & docker @Arguments
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Docker command failed with exit code $LASTEXITCODE"
        }
    }
}

function Get-DockerTarFiles {
    param(
        [string]$TarDirectory,
        [string]$CliName
    )

    Write-Host "Discovering Docker image tar files..."
    $tarPattern = Join-Path $TarDirectory "$CliName-*-image.tar"
    $tarFiles = Get-ChildItem -Path $tarPattern

    if (-not $tarFiles) {
        Write-Host "Directory contents:" -ForegroundColor Yellow
        Get-ChildItem -Path $TarDirectory | ForEach-Object { Write-Host "  $_" }
        Write-Error "No Docker image tar files found matching pattern: $tarPattern"
    }

    Write-Host "Found tar files:"
    # E.g., azmcp-amd64-image.tar, azmcp-arm64-image.tar
    $tarFiles | ForEach-Object { Write-Host "  $($_.FullName)" }
    
    return $tarFiles
}

function Load-DockerImage {
    param([string]$TarPath)
    
    # E.g., "Loading docker_output/azmcp-arm64-image.tar..."
    Write-Host "Loading $TarPath..."
    $output = Invoke-DockerCommand -Arguments @('load', '-i', $TarPath) -CaptureOutput
    
    # Parse "Loaded image: <image-name>" from output
    foreach ($line in $output) {
        if ($line -match 'Loaded image:\s*(.+)$') {
            $imageName = $Matches[1].Trim()
            Write-Host "Loaded image: $imageName"
            # E.g., "azure-sdk/azure-mcp:99999"
            return $imageName
        }
    }
    
    Write-Error "Could not parse loaded image name from docker load output: $output"
}

function Import-ArchitectureImage {
    param(
        [string]$TarPath,
        [string]$CliName,
        [string]$BaseRepo,
        [string]$Version
    )
    
    # Extract architecture from tar filename pattern: {CliName}-{arch}-image.tar
    # E.g., from "azmcp-arm64-image.tar" extract "arm64"
    $fileName = [System.IO.Path]::GetFileName($TarPath)
    $pattern = "^$([regex]::Escape($CliName))-(.+)-image\.tar$"
    if ($fileName -notmatch $pattern) {
        Write-Error "Could not extract architecture from tar file name: $fileName"
    }
    $arch = $Matches[1]
    
    Write-Host "Processing $arch" -ForegroundColor Yellow
    
    $localImage = Load-DockerImage -TarPath $TarPath
    
    # E.g., azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:2.0.0-arm64
    $archTag = "${BaseRepo}:${Version}-${arch}"
    Write-Host "Tagging as: $archTag"
    Invoke-DockerCommand -Arguments @('tag', $localImage, $archTag)
    
    Write-Host ""
    
    return @{
        ArchTag = $archTag
        LocalImage = $localImage
        Architecture = $arch
    }
}

function New-MultiArchManifest {
    param(
        [string]$ManifestTag,
        [string[]]$ArchTags
    )
    
    Write-Host "Creating multi-arch manifest for $ManifestTag..."
    Invoke-DockerCommand -Arguments (@('manifest', 'create', '--amend', $ManifestTag) + $ArchTags)
    Invoke-DockerCommand -Arguments @('manifest', 'push', $ManifestTag)
}

# Main
Write-Host "Docker Release" -ForegroundColor Cyan
Write-Host "CLI Name: $CliName"
Write-Host "Version: $Version"
Write-Host "Base Repo: $BaseRepo"
Write-Host "Tar Directory: $TarDirectory"

Write-Host ""
$tarFiles = Get-DockerTarFiles -TarDirectory $TarDirectory -CliName $CliName

$imageInfos = @()
foreach ($tar in $tarFiles) {
    $info = Import-ArchitectureImage -TarPath $tar.FullName -CliName $CliName -BaseRepo $BaseRepo -Version $Version
    $imageInfos += $info
}

# Create and push tags
Write-Host "Publishing tags" -ForegroundColor Cyan

# E.g., azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:2.0.0
$versionedTag = "${BaseRepo}:${Version}"
# E.g., azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:latest
$latestTag = "${BaseRepo}:latest"

# Push arch-specific tags
$archTags = @()
foreach ($info in $imageInfos) {
    Write-Host "Pushing $($info.ArchTag)..."
    Invoke-DockerCommand -Arguments @('push', $info.ArchTag)
    $archTags += $info.ArchTag
}

# Create and push multi-arch manifests
Write-Host ""
New-MultiArchManifest -ManifestTag $versionedTag -ArchTags $archTags
New-MultiArchManifest -ManifestTag $latestTag -ArchTags $archTags

Write-Host "Publish complete" -ForegroundColor Green
Write-Host "Published tags:"
Write-Host "  - $versionedTag (manifest)"
Write-Host "  - $latestTag (manifest)"
foreach ($tag in $archTags) {
    Write-Host "  - $tag"
}
