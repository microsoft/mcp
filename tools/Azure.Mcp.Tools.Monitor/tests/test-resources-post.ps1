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

# Add static deployment outputs
$staticDeploymentOutputs = @{
    "staticStorageAccountName" = "azuresdktrainingdatatme"
    "staticResourceGroup" = "static-test-resources"
    "staticWorkspace" = "monitor-query-ws"
}

# Merge with existing deployment outputs if they exist
if ($testSettings.DeploymentOutputs) {
    foreach ($key in $staticDeploymentOutputs.Keys) {
        $testSettings.DeploymentOutputs[$key] = $staticDeploymentOutputs[$key]
    }
} else {
    # If DeploymentOutputs doesn't exist, add it as a property
    $testSettings | Add-Member -MemberType NoteProperty -Name "DeploymentOutputs" -Value $staticDeploymentOutputs
}

$storageAccountName = "$($BaseName)mon"
$containerName = 'foo'
$context = New-AzStorageContext -StorageAccountName $storageAccountName -UseConnectedAccount
Write-Host "Uploading sample files to blob storage: $storageAccountName/$containerName" -ForegroundColor Yellow
$files = Get-ChildItem -Path "$PSScriptRoot/samples" -Filter '*.md'
foreach ($file in $files) {
    Set-AzStorageBlobContent -File $file.FullName -Container $containerName -Blob $file.Name -Context $context -Force -ProgressAction SilentlyContinue | Out-Null
}
