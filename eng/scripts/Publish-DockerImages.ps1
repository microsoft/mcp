<#
.SYNOPSIS
    Publishes Docker images to a container registry with multi-architecture manifest support.

.DESCRIPTION
    This script loads Docker images from tar files, tags them with architecture suffixes,
    pushes them to a container registry, and creates multi-arch manifests when multiple
    architectures are present.

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

# E.g., When publishing "Azure.Mcp.Server", given TarDirectory "docker_output" and
# CliName "azmcp", finds docker tar files: azmcp-amd64-image.tar, azmcp-arm64-image.tar
function Get-DockerTarFiles {
    param(
        [string]$TarDirectory,
        [string]$CliName
    )
    
    Write-Host "Discovering tar files..."
    $tarPattern = Join-Path $TarDirectory "$CliName-*-image.tar"
    $tarFiles = Get-ChildItem -Path $tarPattern -ErrorAction SilentlyContinue
    
    if (-not $tarFiles -or $tarFiles.Count -eq 0) {
        Write-Host "ERROR: No tar files found matching pattern $tarPattern" -ForegroundColor Red
        Write-Host "Directory contents:"
        Get-ChildItem -Path $TarDirectory | ForEach-Object { Write-Host "  $_" }
        exit 1
    }
    
    Write-Host "Found tar files:"
    $tarFiles | ForEach-Object { Write-Host "  $($_.FullName)" }
    
    return $tarFiles
}

# E.g., Given TarPath "docker_output/azmcp-arm64-image.tar" and CliName "azmcp",
# returns the architecture suffix "arm64"
function Get-ArchitectureFromTarFile {
    param([string]$TarPath, [string]$CliName)
    
    $fileName = [System.IO.Path]::GetFileName($TarPath)
    # Using the pattern '{CliName}-{arch}-image.tar' capture {arch}
    $pattern = "^$([regex]::Escape($CliName))-(.+)-image\.tar$"
    if ($fileName -match $pattern) {
        return $Matches[1]
    }
    Write-Error "Could not extract architecture from tar file name: $fileName"
}

# Loads Docker image from tar file and returns the image name.
# E.g., Given TarPath "docker_output/azmcp-arm64-image.tar", loads the image and
# returns the image name "azure-sdk/azure-mcp:99999" (DockerLocalTag from build stage)
function Load-DockerImage {
    param([string]$TarPath)
    
    Write-Host "Loading $TarPath..."
    $output = Invoke-DockerCommand -Arguments @('load', '-i', $TarPath) -CaptureOutput
    
    # Parse "Loaded image: <image-name>" from output
    foreach ($line in $output) {
        if ($line -match 'Loaded image:\s*(.+)$') {
            $imageName = $Matches[1].Trim()
            Write-Host "Loaded image: $imageName"
            return $imageName
        }
    }
    
    Write-Error "Could not parse loaded image name from docker load output: $output"
}

# Checks if an image exists in the local Docker daemon.
function Test-DockerImageExists {
    param([string]$ImageName)
    
    $null = docker image inspect $ImageName 2>&1
    return $LASTEXITCODE -eq 0
}

# Loads a Docker image from tar, verifies it exists, and tags it with architecture suffix.
# E.g., for "Azure.Mcp.Server", given TarPath "docker_output/azmcp-arm64-image.tar" 
# creates tag: azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:2.0.0-arm64
# Returns a hashtable with ArchTag, LoadedImage, and Architecture.
function Import-ArchitectureImage {
    param(
        [string]$TarPath,
        [string]$CliName,
        [string]$BaseRepo,
        [string]$Version
    )
    
    $arch = Get-ArchitectureFromTarFile -TarPath $TarPath -CliName $CliName
    Write-Host "Processing $arch" -ForegroundColor Yellow
    
    $localImage = Load-DockerImage -TarPath $TarPath

    if (-not (Test-DockerImageExists -ImageName $localImage)) {
        Write-Error "Loaded image '$localImage' not found after load"
    }
    
    $archTag = "${BaseRepo}:${Version}-${arch}"
    Write-Host "Tagging as: $archTag"
    Invoke-DockerCommand -Arguments @('tag', $localImage, $archTag)
    
    Write-Host ""
    
    return @{
         # E.g., azuresdkimages.azurecr.io/public/azure-sdk/azure-mcp:2.0.0-arm64
        ArchTag = $archTag
         # E.g., azure-sdk/azure-mcp:99999
        LocalImage = $localImage
        # E.g., arm64
        Architecture = $arch
    }
}

# Tags and pushes a single-arch image to the registry.
# E.g., for "Fabric.Mcp.Server", given LocalImage "fabric/fabric-mcp:99999",
# tags and pushes as "azuresdkimages.azurecr.io/public/fabric/fabric-mcp:2.0.0"
function Publish-SingleArchImage {
    param(
        [string]$LocalImage,
        [string]$TargetTag
    )
    
    Write-Host "Tagging $LocalImage as $TargetTag..."
    Invoke-DockerCommand -Arguments @('tag', $LocalImage, $TargetTag)
    Write-Host "Pushing $TargetTag..."
    Invoke-DockerCommand -Arguments @('push', $TargetTag)
}

function New-MultiArchManifest {
    param(
        [string]$ManifestTag,
        [string[]]$ArchTags
    )
    
    Write-Host "Creating multi-arch manifest for $ManifestTag..."
    Invoke-DockerCommand -Arguments (@('manifest', 'create', $ManifestTag) + $ArchTags)
    Invoke-DockerCommand -Arguments @('manifest', 'push', $ManifestTag)
}

# Main
Write-Host "Docker Release" -ForegroundColor Cyan
Write-Host "CLI Name: $CliName"
Write-Host "Version: $Version"
Write-Host "Base Repo: $BaseRepo"
Write-Host "Tar Directory: $TarDirectory"

# Discover Docker image tar files
Write-Host ""
$tarFiles = Get-DockerTarFiles -TarDirectory $TarDirectory -CliName $CliName

# Load and tag each Docker image tar file
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

if ($imageInfos.Count -gt 1) {
    # Multi-arch: push arch-specific tags, then create manifests
    #
    $archTags = @()
    foreach ($info in $imageInfos) {
        Write-Host "Pushing $($info.ArchTag)..."
        Invoke-DockerCommand -Arguments @('push', $info.ArchTag)
        $archTags += $info.ArchTag
    }
    
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
}
else {
    # Single arch: just tag and push versioned/latest directly (no arch-specific tag published)
    #
    Write-Host "Single architecture - publishing directly..."
    $localImage = $imageInfos[0].LocalImage
    
    Publish-SingleArchImage -LocalImage $localImage -TargetTag $versionedTag
    Publish-SingleArchImage -LocalImage $localImage -TargetTag $latestTag
    
    Write-Host "Publish complete" -ForegroundColor Green
    Write-Host "Published tags:"
    Write-Host "  - $versionedTag"
    Write-Host "  - $latestTag"
}
