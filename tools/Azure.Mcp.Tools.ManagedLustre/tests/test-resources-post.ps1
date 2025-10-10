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

Install-Module -Name Az.StorageCache -Repository PSGallery -Scope CurrentUser -Force

$amlfsName = $testSettings.ResourceBaseName

Write-Host "Verifying AMLFS cluster deployment: $amlfsName" -ForegroundColor Yellow

# Get the AMLFS instance details to verify deployment
$amlfsCluster = Get-AzStorageCacheAmlFileSystem -ResourceGroupName $ResourceGroupName -Name $amlfsName

if ($amlfsCluster) {
    Write-Host "Azure Managed Lustre cluster '$amlfsName' deployed successfully" -ForegroundColor Green
    Write-Host "  Name: $($amlfsCluster.Name)" -ForegroundColor Gray
    Write-Host "  ID: $($amlfsCluster.Id)" -ForegroundColor Gray
    Write-Host "  Sku: $($amlfsCluster.SkuName)" -ForegroundColor Gray
    Write-Host "  Size: $($amlfsCluster.StorageCapacityTiB)" -ForegroundColor Gray
    Write-Host "  Location: $($amlfsCluster.Location)" -ForegroundColor Gray
} else {
    Write-Error "AMLFS Cluster '$amlfsName' not found"
}

# Retrieve principal ID for "HPC Cache Resource Provider" and assign roles on the storage account
# This is not easy to do in Bicep and at the resource group scope
Write-Host "Resolving 'HPC Cache Resource Provider' service principal using the network cards of AMLFS instance..." -ForegroundColor Yellow

# Ensure required modules are available
if (-not (Get-Command -Name Get-AzNetworkInterface -ErrorAction SilentlyContinue)) {
    Install-Module -Name Az.Network -Repository PSGallery -Scope CurrentUser -Force
}
if (-not (Get-Command -Name Get-AzActivityLog -ErrorAction SilentlyContinue)) {
    Install-Module -Name Az.Monitor -Repository PSGallery -Scope CurrentUser -Force
}

# Find the first NIC starting with 'amlfs'
$nic = Get-AzNetworkInterface -ResourceGroupName $ResourceGroupName -ErrorAction Stop |
    Where-Object { $_.Name -like 'amlfs*' } |
    Sort-Object Name |
    Select-Object -First 1

if (-not $nic) {
    Write-Error "No network interfaces starting with 'amlfs' found in resource group '$ResourceGroupName'."
}

Write-Host "Selected NIC: $($nic.Name)" -ForegroundColor Yellow

# Get the first (earliest) activity log entry for the NIC and extract the caller
$startTime = (Get-Date).AddDays(-7)
$events = Get-AzActivityLog -ResourceId $nic.Id -StartTime $startTime -ErrorAction Stop

if (-not $events) {
    Write-Error "No activity log events found for '$($nic.Name)' since $startTime." -ForegroundColor Yellow
} else {
    $firstEvent = $events | Sort-Object EventTimestamp | Select-Object -First 1
    $opName = $firstEvent.OperationName.LocalizedValue
    if (-not $opName) { $opName = $firstEvent.OperationName.Value }

    Write-Host "First operation on resource: $opName" -ForegroundColor Gray
    Write-Host "Caller: $($firstEvent.Caller)" -ForegroundColor Green
}


$storageAccountName = $testSettings.ResourceBaseName

$sa = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $storageAccountName -ErrorAction Stop
$scope = $sa.Id

$rolesToAssign = @(
    "Storage Account Contributor",
    "Storage Blob Data Contributor"
)

$HPCCacheResourceProviderPrincipalId = $firstEvent.Caller

foreach ($role in $rolesToAssign) {
    Write-Host "Assigning role '$role' to principal 'HPC Cache Resource Provider'on scope '$scope'..." -ForegroundColor Yellow
    New-AzRoleAssignment -Scope $scope -RoleDefinitionName $role -PrincipalId $HPCCacheResourceProviderPrincipalId | Out-Null
}

# ---------- Below is required because HSM settings are not applied because can't list service principals ----------
Write-Host "Recreating AMLFS instance to ensure required HSM settings are applied (always recreate strategy)..." -ForegroundColor Yellow

# Capture desired parameters from existing instance if present so we preserve SKU/capacity/location/zone
$existingSku = $amlfsCluster?.SkuName
$existingCapacity = $amlfsCluster?.StorageCapacityTiB
$existingLocation = $amlfsCluster?.Location

# Derive expected HSM container resource IDs from deployment outputs (preferred) with fallback
$expectedHsmContainerId    = $DeploymentOutputs['HSM_CONTAINER_ID']
$expectedHsmLogContainerId = $DeploymentOutputs['HSM_LOGS_CONTAINER_ID']

if (-not $expectedHsmContainerId -or -not $expectedHsmLogContainerId) {
    Write-Warning "DeploymentOutputs missing HSM container IDs; falling back to constructing from storage account ID."
    $storageAccountId = $sa.Id
    $fallbackDataName = 'hsm-data'
    $fallbackLogsName = 'hsm-logs'
    if (-not $expectedHsmContainerId)    { $expectedHsmContainerId    = "$storageAccountId/blobServices/default/containers/$fallbackDataName" }
    if (-not $expectedHsmLogContainerId) { $expectedHsmLogContainerId = "$storageAccountId/blobServices/default/containers/$fallbackLogsName" }
}

# Preserve / resolve subnet before deletion (prefer deployment output)
$subnetId = $DeploymentOutputs['AMLFS_SUBNET_ID']
if (-not $subnetId) {
    # Fall back to discovering from existing NIC
    $subnetId = $nic.IpConfigurations[0].Subnet.Id
    Write-Warning "AMLFS_SUBNET_ID not found in DeploymentOutputs; using NIC-derived subnet id."
}

if ($amlfsCluster) {
    Write-Host "Deleting existing AMLFS '$amlfsName'..." -ForegroundColor Yellow
    try {
        Remove-AzStorageCacheAmlFileSystem -ResourceGroupName $ResourceGroupName -Name $amlfsName -ErrorAction Stop
    } catch {
        Write-Error "Failed to initiate deletion of AMLFS '$amlfsName': $($_.Exception.Message)"; throw
    }

    # Poll for deletion completion (up to ~20 minutes)
    $pollSeconds = 30
    for ($i=0; $i -lt 40; $i++) {
        Start-Sleep -Seconds $pollSeconds
        $exists = Get-AzStorageCacheAmlFileSystem -ResourceGroupName $ResourceGroupName -Name $amlfsName -ErrorAction SilentlyContinue
        if (-not $exists) { Write-Host "Deletion confirmed." -ForegroundColor Gray; break }
        Write-Host "Waiting for AMLFS deletion (attempt $($i+1))..." -ForegroundColor DarkGray
        if ($i -eq 39) { Write-Warning "Timed out waiting for AMLFS deletion; will attempt creation anyway (may fail)." }
    }
}
else {
    Write-Host "No existing AMLFS instance found; proceeding to create a fresh one." -ForegroundColor Gray
}

# Fallback defaults if original values were not present
if (-not $existingSku) { $existingSku = "AMLFS-Durable-Premium-500" }
if (-not $existingCapacity -or $existingCapacity -le 0) { $existingCapacity = 4 } # Align with template default (4 TiB)
if (-not $existingLocation) { $existingLocation = $sa.Location }
if (-not $existingLocation) { $existingLocation = 'westus' }

# Override location from deployment outputs if provided
$locationFromOutputs = $DeploymentOutputs['LOCATION']
if ($locationFromOutputs) {
    $location = $locationFromOutputs
} else {
    $location = $existingLocation  # ensure variable used in creation call is defined
    if (-not $locationFromOutputs) { Write-Host "LOCATION output not provided; using inferred location '$location'." -ForegroundColor Gray }
}

Write-Host "Creating AMLFS '$amlfsName' (Sku=$existingSku, Capacity=${existingCapacity}TiB, Location=$existingLocation, Zone=1)" -ForegroundColor Yellow
Write-Host "  HSM Data Container:    $expectedHsmContainerId" -ForegroundColor Gray
Write-Host "  HSM Logging Container: $expectedHsmLogContainerId" -ForegroundColor Gray

try {
    New-AzStorageCacheAmlFileSystem `
        -Name $amlfsName `
        -ResourceGroupName $ResourceGroupName `
        -Location $location `
        -SkuName $existingSku `
        -StorageCapacityTiB $existingCapacity `
        -FilesystemSubnet $subnetId `
        -SettingContainer $expectedHsmContainerId `
        -SettingLoggingContainer $expectedHsmLogContainerId `
        -MaintenanceWindowDayOfWeek Monday `
        -MaintenanceWindowTimeOfDayUtc "12:00" `
        -Confirm:$false | Out-Null
    Write-Host "AMLFS '$amlfsName' recreated successfully with required HSM settings." -ForegroundColor Green
} catch {
    Write-Error "Failed to (re)create AMLFS '$amlfsName': $($_.Exception.Message)"; throw
}