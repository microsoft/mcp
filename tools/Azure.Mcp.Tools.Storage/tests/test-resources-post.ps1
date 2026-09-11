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

$vmName = $testSettings.DeploymentOutputs['VMNAME']
Write-Host "Storage disk diagnosis VM: $vmName" -ForegroundColor Cyan
Write-Host "Waiting for the Storage disk diagnosis VM to be ready..." -ForegroundColor Yellow

$vmReady = $false
for ($retry = 1; $retry -le 30; $retry++) {
    $vm = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $vmName -Status
    $powerState = $vm.Statuses | Where-Object { $_.Code -like 'PowerState/*' } | Select-Object -First 1

    if ($vm.ProvisioningState -eq 'Succeeded' -and $powerState.Code -eq 'PowerState/running') {
        $vmReady = $true
        break
    }

    Write-Host "  Retry $retry/30 - Provisioning: $($vm.ProvisioningState), Power: $($powerState.Code)" -ForegroundColor Gray
    Start-Sleep -Seconds 10
}

if (-not $vmReady) {
    throw "Storage disk diagnosis VM '$vmName' did not become ready within five minutes."
}

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
