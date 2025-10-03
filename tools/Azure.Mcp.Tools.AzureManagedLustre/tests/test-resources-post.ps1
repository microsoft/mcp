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