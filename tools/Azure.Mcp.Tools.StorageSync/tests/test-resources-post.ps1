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
# Try both camelCase and UPPERCASE keys for backwards compatibility
$syncGroupName = if ($DeploymentOutputs.ContainsKey('syncGroupName')) { 
    $DeploymentOutputs['syncGroupName'] 
} elseif ($DeploymentOutputs.ContainsKey('SYNCGROUPNAME')) { 
    $DeploymentOutputs['SYNCGROUPNAME'] 
} else { 
    $BaseName 
}

$cloudEndpointName = if ($DeploymentOutputs.ContainsKey('cloudEndpointName')) { 
    $DeploymentOutputs['cloudEndpointName'] 
} elseif ($DeploymentOutputs.ContainsKey('CLOUDENDPOINTNAME')) { 
    $DeploymentOutputs['CLOUDENDPOINTNAME'] 
} else { 
    $BaseName 
}

Write-Host "Setting up Storage Sync Service for testing: $storageSyncServiceName" -ForegroundColor Yellow
Write-Host "Sync Group Name: $syncGroupName" -ForegroundColor Gray
Write-Host "Cloud Endpoint Name: $cloudEndpointName" -ForegroundColor Gray

try {
    # Check if Storage Sync Service exists
    $storageSyncService = Get-AzStorageSyncService -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -ErrorAction SilentlyContinue

    if (-not $storageSyncService) {
        Write-Warning "Storage Sync Service '$storageSyncServiceName' not found in resource group '$ResourceGroupName'"
        return
    }

    Write-Host "Storage Sync Service found: $($storageSyncService.Id)" -ForegroundColor Green

    # Storage Sync Agent registration is only supported on Windows
    $registeredServer = $null
    if ($IsWindows -or ($PSVersionTable.PSVersion.Major -lt 6)) {
        # Check if running as administrator (Windows only)
        $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

        if (-not $isAdmin) {
            $errorMessage = @"
ERROR: Register server failed due to insufficient privileges. Run this command instead:

Start-Process pwsh -Verb RunAs -ArgumentList "-NoExit -Command cd $PSScriptRoot\..\..\..; ./eng/scripts/Deploy-TestResources.ps1 -Paths StorageSync"
"@
            Write-Error $errorMessage -ErrorAction Stop
        }

        # Import Storage Sync module and reset server
        Import-Module "C:\Program Files\Azure\StorageSyncAgent\StorageSync.Management.ServerCmdlets.dll"
        Write-Host "Register Server is resetting existing server configuration" -ForegroundColor Gray
        Reset-StorageSyncServer -Force -Verbose -ErrorAction SilentlyContinue

        # Register a RegisteredServer (Note: This requires the Storage Sync Agent to be installed on a server)
        Write-Host "Attempting to register server with Storage Sync Service (requires Storage Sync Agent installed)" -ForegroundColor Gray
        $registeredServer = $storageSyncService | Register-AzStorageSyncServer -Verbose
        Write-Host "Attempted to register server with Storage Sync Service (requires Storage Sync Agent installed)" -ForegroundColor Gray
    }
    else {
        Write-Host "Skipping server registration (Windows-only feature, running on $($PSVersionTable.Platform))" -ForegroundColor Yellow
    }

    # Get Sync Group from deployment outputs
    if ([string]::IsNullOrWhiteSpace($syncGroupName)) {
        Write-Warning "Sync Group name is not available from deployment outputs, using BaseName as fallback"
        $syncGroupName = $BaseName
    }

    $syncGroup = Get-AzStorageSyncGroup -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -SyncGroupName $syncGroupName -ErrorAction SilentlyContinue

    if ($syncGroup) {
        Write-Host "Sync Group found: $syncGroupName" -ForegroundColor Green
    }
    else {
        Write-Warning "Sync Group '$syncGroupName' not found"
    }

    # Get Cloud Endpoint if it exists
    if ([string]::IsNullOrWhiteSpace($cloudEndpointName)) {
        Write-Warning "Cloud Endpoint name is not available from deployment outputs, using BaseName as fallback"
        $cloudEndpointName = $BaseName
    }

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
    if ($registeredServer) {
        $registeredServerId = $registeredServer.ServerId
        $registeredServer = Get-AzStorageSyncServer -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -ServerId $registeredServerId -ErrorAction SilentlyContinue

        if ($registeredServer) {
            Write-Host "Registered Server found: $registeredServerId" -ForegroundColor Green
            Write-Host "  - Server Id: $($registeredServer.ServerId)" -ForegroundColor Gray
            Write-Host "  - Friendly Name: $($registeredServer.FriendlyName)" -ForegroundColor Gray
        }
        else {
            Write-Host "Registered Server '$registeredServerId' not yet available (requires Storage Sync Agent)" -ForegroundColor Yellow
        }

        # create a new server endpoint if needed
        $serverEndpointName = "$BaseName"
        $serverLocalPath = "D:\$serverEndpointName"

        New-AzStorageSyncServerEndpoint -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -SyncGroupName $syncGroupName -Name $serverEndpointName -ServerResourceId $registeredServer.ResourceId -ServerLocalPath $serverLocalPath -ErrorAction SilentlyContinue | Out-Null

        # Get Server Endpoint if it exists
        $serverEndpoint = Get-AzStorageSyncServerEndpoint -ResourceGroupName $ResourceGroupName -StorageSyncServiceName $storageSyncServiceName -SyncGroupName $syncGroupName -Name $serverEndpointName -ErrorAction SilentlyContinue

        if ($serverEndpoint) {
            Write-Host "Server Endpoint found: $serverEndpointName" -ForegroundColor Green
            Write-Host "  - Server Local Path: $($serverEndpoint.ServerLocalPath)" -ForegroundColor Gray
            Write-Host "  - Cloud Tiering: $($serverEndpoint.CloudTiering)" -ForegroundColor Gray
        }
        else {
            Write-Host "Server Endpoint '$serverEndpointName' not yet available (requires active registered server)" -ForegroundColor Yellow
        }
    }
    else {
        Write-Host "Skipping server endpoint creation (no registered server available)" -ForegroundColor Yellow
    }

    Write-Host "Storage Sync Service setup completed successfully" -ForegroundColor Green
}
catch {
    Write-Error "Error setting up Storage Sync Service: $_" -ErrorAction Stop
}
