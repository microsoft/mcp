<#
.SYNOPSIS
    Loads Docker images from tar files and tags them with local static tags.

.DESCRIPTION
    This script loads Docker images from tar files produced by the build pipeline,
    then tags each with a predictable local tag ({ImageName}:{arch}) for use by
    1ES.PushContainerImage tasks.

.PARAMETER CliName
    The CLI name used to identify tar files (e.g., 'azmcp' matches 'azmcp-amd64-image.tar').

.PARAMETER ImageName
    The Docker image name used for local tagging (e.g., 'azure-sdk/azure-mcp').

.PARAMETER TarDirectory
    The directory containing the Docker image tar files.

.EXAMPLE
    ./Load-DockerImages.ps1 -CliName 'azmcp' -ImageName 'azure-sdk/azure-mcp' -TarDirectory './docker_output'
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$CliName,

    [Parameter(Mandatory = $true)]
    [string]$ImageName,

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

# Main
Write-Host "Docker Image Load" -ForegroundColor Cyan
Write-Host "CLI Name: $CliName"
Write-Host "Image Name: $ImageName"
Write-Host "Tar Directory: $TarDirectory"
Write-Host ""

Write-Host "Discovering Docker image tar files..."
$tarPattern = Join-Path $TarDirectory "$CliName-*-image.tar"
$tarFiles = Get-ChildItem -Path $tarPattern

if (-not $tarFiles) {
    Write-Host "Directory contents:" -ForegroundColor Yellow
    Get-ChildItem -Path $TarDirectory | ForEach-Object { Write-Host "  $_" }
    Write-Error "No Docker image tar files found matching pattern: $tarPattern"
}

Write-Host "Found tar files:"
$tarFiles | ForEach-Object { Write-Host "  $($_.FullName)" }
Write-Host ""

foreach ($tar in $tarFiles) {
    # Extract architecture from tar filename pattern: {CliName}-{arch}-image.tar
    # E.g., from "azmcp-arm64-image.tar" extract "arm64"
    $fileName = $tar.Name
    $pattern = "^$([regex]::Escape($CliName))-(.+)-image\.tar$"
    if ($fileName -notmatch $pattern) {
        Write-Error "Could not extract architecture from tar file name: $fileName"
    }
    $arch = $Matches[1]

    Write-Host "Processing $arch" -ForegroundColor Yellow

    # Load the image from tar
    Write-Host "Loading $($tar.FullName)..."
    $output = Invoke-DockerCommand -Arguments @('load', '-i', $tar.FullName) -CaptureOutput

    $loadedImage = $null
    foreach ($line in $output) {
        if ($line -match 'Loaded image:\s*(.+)$') {
            $loadedImage = $Matches[1].Trim()
            Write-Host "Loaded image: $loadedImage"
            break
        }
    }

    if (-not $loadedImage) {
        Write-Error "Could not parse loaded image name from docker load output: $output"
    }

    # Tag with static local name: {ImageName}:{arch}
    # E.g., azure-sdk/azure-mcp:amd64, azure-sdk/azure-mcp:arm64
    $localTag = "${ImageName}:${arch}"
    Write-Host "Tagging as: $localTag"
    Invoke-DockerCommand -Arguments @('tag', $loadedImage, $localTag)
    Write-Host ""
}

Write-Host "Load complete" -ForegroundColor Green
Write-Host "Local tags:"
foreach ($tar in $tarFiles) {
    $fileName = $tar.Name
    if ($fileName -match "^$([regex]::Escape($CliName))-(.+)-image\.tar$") {
        Write-Host "  - ${ImageName}:$($Matches[1])"
    }
}
