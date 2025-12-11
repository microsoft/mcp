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
