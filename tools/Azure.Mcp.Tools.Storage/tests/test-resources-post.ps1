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

# Write a blob to storage
$context = New-AzStorageContext -StorageAccountName $testSettings.ResourceBaseName -UseConnectedAccount

Write-Host "Uploading README.md to blob storage: $BaseName/bar" -ForegroundColor Yellow
Set-AzStorageBlobContent `
    -File "$RepoRoot/README.md" `
    -Container "bar" `
    -Blob "README.md" `
    -Context $context `
    -Force `
    -ProgressAction SilentlyContinue
| Out-Null

# Write a file to file share storage

# File share needs to use EnableFileBackupRequestIntent
$context = New-AzStorageContext -StorageAccountName $testSettings.ResourceBaseName -UseConnectedAccount -EnableFileBackupRequestIntent

Write-Host "Uploading README.md to file share: testshare" -ForegroundColor Yellow
Set-AzStorageFileContent `
    -ShareName "testshare" `
    -Source "$RepoRoot/README.md" `
    -Path "README.md" `
    -Context $context `
    -Force `
    -ProgressAction SilentlyContinue
| Out-Null
