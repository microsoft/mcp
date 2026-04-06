param(
    [string] $TenantId,
    [string] $TestApplicationId,
    [string] $ResourceGroupName,
    [string] $BaseName,
    [hashtable] $DeploymentOutputs,
    [hashtable] $AdditionalParameters
)

$ErrorActionPreference = "Stop"

. "$PSScriptRoot/../../../eng/common/scripts/common.ps1"
. "$PSScriptRoot/../../../eng/scripts/helpers/TestResourcesHelpers.ps1"

$testSettings = New-TestSettings @PSBoundParameters -OutputPath $PSScriptRoot

# Write a blob to storage
$isGovCloud = $DeploymentOutputs['ENVIRONMENT'] -eq 'AzureUSGovernment'
$storageEndpointSuffix = if ($isGovCloud) { 'core.usgovcloudapi.net' } else { 'core.windows.net' }
$context = New-AzStorageContext -StorageAccountName $testSettings.ResourceBaseName -UseConnectedAccount -Endpoint $storageEndpointSuffix

Write-Host "Uploading README.md to blob storage: $BaseName/bar" -ForegroundColor Yellow
Set-AzStorageBlobContent `
    -File "$RepoRoot/README.md" `
    -Container "bar" `
    -Blob "README.md" `
    -Context $context `
    -Force `
    -ProgressAction SilentlyContinue
| Out-Null
