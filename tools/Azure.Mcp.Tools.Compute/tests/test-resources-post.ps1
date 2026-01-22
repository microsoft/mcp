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
Write-Host "  VM Name: $($DeploymentOutputs['vmName'].Value)" -ForegroundColor Cyan
Write-Host "  VM2 Name: $($DeploymentOutputs['vm2Name'].Value)" -ForegroundColor Cyan
Write-Host "  VMSS Name: $($DeploymentOutputs['vmssName'].Value)" -ForegroundColor Cyan
Write-Host "  Resource Group: $($DeploymentOutputs['resourceGroupName'].Value)" -ForegroundColor Cyan

# Wait for VMs to be fully provisioned and running
Write-Host "Waiting for VMs to be fully provisioned..." -ForegroundColor Yellow

$maxRetries = 30
$retryCount = 0
$allVmsRunning = $false

while (-not $allVmsRunning -and $retryCount -lt $maxRetries) {
    $retryCount++

    try {
        # Check VM status
        $vm1 = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $DeploymentOutputs['vmName'].Value -Status
        $vm2 = Get-AzVM -ResourceGroupName $ResourceGroupName -Name $DeploymentOutputs['vm2Name'].Value -Status

        $vm1Status = $vm1.Statuses | Where-Object { $_.Code -like "PowerState/*" } | Select-Object -First 1
        $vm2Status = $vm2.Statuses | Where-Object { $_.Code -like "PowerState/*" } | Select-Object -First 1

        if ($vm1Status.Code -eq "PowerState/running" -and $vm2Status.Code -eq "PowerState/running") {
            $allVmsRunning = $true
            Write-Host "✓ All VMs are running" -ForegroundColor Green
        }
        else {
            Write-Host "  Retry $retryCount/$maxRetries - VM1: $($vm1Status.Code), VM2: $($vm2Status.Code)" -ForegroundColor Gray
            Start-Sleep -Seconds 10
        }
    }
    catch {
        Write-Host "  Retry $retryCount/$maxRetries - Waiting for VM status..." -ForegroundColor Gray
        Start-Sleep -Seconds 10
    }
}

if (-not $allVmsRunning) {
    Write-Warning "VMs did not reach running state within timeout period. Tests may need to wait for VMs to be ready."
}

Write-Host ""
Write-Host "Test settings written to: $PSScriptRoot/.testsettings.json" -ForegroundColor Green
Write-Host ""
Write-Host "To run live tests:" -ForegroundColor Yellow
Write-Host "  ./eng/scripts/Test-Code.ps1 -TestType Live -Paths Compute" -ForegroundColor Cyan
