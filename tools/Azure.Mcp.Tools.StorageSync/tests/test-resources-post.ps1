param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

$storageSyncServiceName = $BaseName

Write-Host "Setting up Storage Sync Service for testing: $storageSyncServiceName" -ForegroundColor Yellow

try {
    # Check if Storage Sync Service exists
    $storageSyncService = Get-AzStorageSyncService -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -ErrorAction SilentlyContinue

    if (-not $storageSyncService) {
        Write-Warning "Storage Sync Service '$storageSyncServiceName' not found in resource group '$ResourceGroupName'"
        return
    }

    Write-Host "Storage Sync Service found: $($storageSyncService.Id)" -ForegroundColor Green

    # Get Sync Group
    $syncGroupName = "$BaseName-sg"
    $syncGroup = Get-AzStorageSyncGroup -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -SyncGroupName $syncGroupName -ErrorAction SilentlyContinue

    if ($syncGroup) {
        Write-Host "Sync Group found: $syncGroupName" -ForegroundColor Green
    }
    else {
        Write-Warning "Sync Group '$syncGroupName' not found"
    }

    # Get Cloud Endpoint if it exists
    $cloudEndpointName = "$BaseName-ce"
    $cloudEndpoint = Get-AzStorageSyncCloudEndpoint -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -SyncGroupName $syncGroupName -Name $cloudEndpointName -ErrorAction SilentlyContinue

    if ($cloudEndpoint) {
        Write-Host "Cloud Endpoint found: $cloudEndpointName" -ForegroundColor Green
        Write-Host "  - Azure File Share: $($cloudEndpoint.AzureFileShareName)" -ForegroundColor Gray
        Write-Host "  - Status: $($cloudEndpoint.LastOperationName)" -ForegroundColor Gray
    }
    else {
        Write-Host "Cloud Endpoint '$cloudEndpointName' not yet available (this is normal during initial setup)" -ForegroundColor Yellow
    }

    Write-Host "Storage Sync Service setup completed successfully" -ForegroundColor Green
}
catch {
    Write-Error "Error setting up Storage Sync Service: $_" -ErrorAction Stop
}
