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
$context = New-AzStorageContext -StorageAccountName $testSettings.ResourceBaseName -UseConnectedAccount

foreach ($blob in @(
    "README.md",
    "hierarchy/root.txt",
    "hierarchy/second.txt",
    "hierarchy/subfolder/child.txt",
    "hierarchy/subfolder/nested/deep.txt"
)) {
    Write-Host "Uploading $blob to blob storage: $BaseName/bar" -ForegroundColor Yellow
    Set-AzStorageBlobContent `
        -File "$RepoRoot/README.md" `
        -Container "bar" `
        -Blob $blob `
        -Context $context `
        -Force `
        -ProgressAction SilentlyContinue
    | Out-Null
}
