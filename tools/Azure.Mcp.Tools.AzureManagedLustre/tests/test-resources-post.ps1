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
Write-Host "Resolving 'HPC Cache Resource Provider' service principal..." -ForegroundColor Yellow

$storageAccountName = $testSettings.ResourceBaseName

$sa = Get-AzStorageAccount -ResourceGroupName $ResourceGroupName -Name $storageAccountName -ErrorAction Stop
$scope = $sa.Id

$rolesToAssign = @(
    "Storage Account Contributor",
    "Storage Blob Data Contributor"
)

$HPCCacheResourceProviderApplicationId = '4392ab71-2ce2-4b0d-8770-b352745c73f5'

foreach ($role in $rolesToAssign) {
    Write-Host "Assigning role '$role' to principal 'HPC Cache Resource Provider'on scope '$scope'..." -ForegroundColor Yellow
    New-AzRoleAssignment -Scope $scope -RoleDefinitionName $role -ApplicationId $HPCCacheResourceProviderApplicationId -Debug | Out-Null
}
