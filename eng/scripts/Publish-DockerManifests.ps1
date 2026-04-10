<#
.SYNOPSIS
    Creates and pushes multi-architecture Docker manifests to a container registry.

.DESCRIPTION
    This script creates multi-arch manifests for versioned and latest tags,
    referencing architecture-specific images that have already been pushed to the registry.

.PARAMETER Version
    The version tag for the Docker images (e.g., '2.0.0').

.PARAMETER BaseRepo
    The base repository URL without tag (e.g., 'azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp').

.PARAMETER Architectures
    The architectures to include in the manifest (e.g., 'amd64', 'arm64').

.EXAMPLE
    ./Publish-DockerManifests.ps1 -Version '2.0.0' -BaseRepo 'azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp' -Architectures 'amd64','arm64'
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$BaseRepo,

    [Parameter(Mandatory = $true)]
    [string[]]$Architectures
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-DockerCommand {
    param(
        [string[]]$Arguments
    )

    Write-Host "docker $($Arguments -join ' ')" -ForegroundColor DarkGray
    & docker @Arguments
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Docker command failed with exit code $LASTEXITCODE"
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
Write-Host "Docker Multi-Arch Manifest Publish" -ForegroundColor Cyan
Write-Host "Version: $Version"
Write-Host "Base Repo: $BaseRepo"
Write-Host "Architectures: $($Architectures -join ', ')"
Write-Host ""

# Build arch-specific tag list from architectures
# E.g., azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:2.0.0-amd64
$archTags = $Architectures | ForEach-Object { "${BaseRepo}:${Version}-${_}" }

Write-Host "Architecture-specific tags:"
foreach ($tag in $archTags) {
    Write-Host "  - $tag"
}
Write-Host ""

# E.g., azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:2.0.0
$versionedTag = "${BaseRepo}:${Version}"
# E.g., azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:latest
$latestTag = "${BaseRepo}:latest"

# Create and push multi-arch manifests
New-MultiArchManifest -ManifestTag $versionedTag -ArchTags $archTags
New-MultiArchManifest -ManifestTag $latestTag -ArchTags $archTags

Write-Host ""
Write-Host "Manifest publish complete" -ForegroundColor Green
Write-Host "Published manifests:"
Write-Host "  - $versionedTag"
Write-Host "  - $latestTag"
