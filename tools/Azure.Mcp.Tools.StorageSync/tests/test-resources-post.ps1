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

    # Get Registered Server if it exists
    $registeredServerName = "$BaseName-rs"
    $registeredServer = Get-AzStorageSyncServer -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -ServerName $registeredServerName -ErrorAction SilentlyContinue

    if ($registeredServer) {
        Write-Host "Registered Server found: $registeredServerName" -ForegroundColor Green
        Write-Host "  - Server Name: $($registeredServer.ServerName)" -ForegroundColor Gray
        Write-Host "  - Friendly Name: $($registeredServer.FriendlyName)" -ForegroundColor Gray
    }
    else {
        Write-Host "Registered Server '$registeredServerName' not yet available (requires Storage Sync Agent)" -ForegroundColor Yellow
    }

    # Get Server Endpoint if it exists
    $serverEndpointName = "$BaseName-se"
    $serverEndpoint = Get-AzStorageSyncServerEndpoint -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -SyncGroupName $syncGroupName -Name $serverEndpointName -ErrorAction SilentlyContinue

    if ($serverEndpoint) {
        Write-Host "Server Endpoint found: $serverEndpointName" -ForegroundColor Green
        Write-Host "  - Server Local Path: $($serverEndpoint.ServerLocalPath)" -ForegroundColor Gray
        Write-Host "  - Cloud Tiering: $($serverEndpoint.CloudTiering)" -ForegroundColor Gray
    }
    else {
        Write-Host "Server Endpoint '$serverEndpointName' not yet available (requires active registered server)" -ForegroundColor Yellow
    }

    Write-Host "Storage Sync Service setup completed successfully" -ForegroundColor Green
}
catch {
    Write-Error "Error setting up Storage Sync Service: $_" -ErrorAction Stop
}

