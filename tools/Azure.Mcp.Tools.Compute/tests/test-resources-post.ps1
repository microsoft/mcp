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

Write-Host "Compute test resources deployed successfully" -ForegroundColor Green
Write-Host "  VM Name: $($DeploymentOutputs['VMNAME'].Value)" -ForegroundColor Cyan
Write-Host "  VMSS Name: $($DeploymentOutputs['VMSSNAME'].Value)" -ForegroundColor Cyan
Write-Host "  Resource Group: $($DeploymentOutputs['RESOURCEGROUPNAME'].Value)" -ForegroundColor Cyan
Write-Host "  Disk Name: $($DeploymentOutputs['diskName'].Value)" -ForegroundColor Cyan

# Wait for VM to be fully provisioned and running
Write-Host "Waiting for VM to be fully provisioned..." -ForegroundColor Yellow

$maxRetries = 30
$retryCount = 0
$vmRunning = $false

while (-not $vmRunning -and $retryCount -lt $maxRetries) {
    $retryCount++

    try {
        # Check VM status
        $vm = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $DeploymentOutputs['VMNAME'].Value -Status

        $vmStatus = $vm.Statuses | Where-Object { $_.Code -like "PowerState/*" } | Select-Object -First 1

        if ($vmStatus.Code -eq "PowerState/running") {
            $vmRunning = $true
            Write-Host "✓ VM is running" -ForegroundColor Green
        }
        else {
            Write-Host "  Retry $retryCount/$maxRetries - VM: $($vmStatus.Code)" -ForegroundColor Gray
            Start-Sleep -Seconds 10
        }
    }
    catch {
        Write-Host "  Retry $retryCount/$maxRetries - Waiting for VM status..." -ForegroundColor Gray
        Start-Sleep -Seconds 10
    }
}

if (-not $vmRunning) {
    Write-Warning "VM did not reach running state within timeout period. Tests may need to wait for VM to be ready."
}

Write-Host ""
Write-Host "Test settings written to: $PSScriptRoot/.testsettings.json" -ForegroundColor Green
Write-Host ""
Write-Host "To run live tests:" -ForegroundColor Yellow
Write-Host "  ./eng/scripts/Test-Code.ps1 -TestType Live -Paths Compute" -ForegroundColor Cyan
