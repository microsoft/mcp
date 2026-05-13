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

Write-Host "Azure Backup test resources deployed successfully for vault: $BaseName-rsv" -ForegroundColor Yellow

# ─── Setup for Undelete Tests ───
# Creates backup instances in both RSV and DPP, then soft-deletes them
# so the undelete live tests have real soft-deleted items to restore.

$subscriptionId = (Get-AzContext).Subscription.Id
$rsvVaultName = "$BaseName-rsv"
$dppVaultName = "$BaseName-dpp"
# DeploymentOutputs is a case-sensitive OrderedDictionary (from ConvertFrom-Json -AsHashtable)
# and the test resources framework upper-cases all output keys before serializing.
$diskId = $DeploymentOutputs["DISKID"]
$storageAccountName = $DeploymentOutputs["STORAGEACCOUNTNAME"]
$fileShareName = $DeploymentOutputs["FILESHARENAME"]
$storageAccountId = $DeploymentOutputs["STORAGEACCOUNTID"]

Write-Host "Setting up undelete test infrastructure..." -ForegroundColor Cyan

# ── DPP: Create disk backup policy and protect disk ──
Write-Host "Creating DPP disk backup policy..." -ForegroundColor Yellow
$dppPolicyName = "undelete-disk-policy"

try {
    $existingPolicy = Get-AzDataProtectionBackupPolicy -VaultName $dppVaultName -ResourceGroupName $ResourceGroupName -Name $dppPolicyName -ErrorAction SilentlyContinue
} catch {
    $existingPolicy = $null
}

if (-not $existingPolicy) {
    $policyDefn = Get-AzDataProtectionPolicyTemplate -DatasourceType AzureDisk
    $policyDefn.PolicyRule[0].Trigger.ScheduleRepeatingTimeInterval = @("R/2024-01-01T02:00:00+00:00/P1D")
    New-AzDataProtectionBackupPolicy -VaultName $dppVaultName -ResourceGroupName $ResourceGroupName -Name $dppPolicyName -Policy $policyDefn
    Write-Host "DPP disk backup policy created." -ForegroundColor Green
} else {
    Write-Host "DPP disk backup policy already exists." -ForegroundColor Gray
}

Write-Host "Protecting disk in DPP vault..." -ForegroundColor Yellow
$dppPolicy = Get-AzDataProtectionBackupPolicy -VaultName $dppVaultName -ResourceGroupName $ResourceGroupName -Name $dppPolicyName

try {
    $existingInstance = Get-AzDataProtectionBackupInstance -VaultName $dppVaultName -ResourceGroupName $ResourceGroupName | Where-Object {
        $_.Property.DataSourceInfo.ResourceId -eq $diskId
    }
} catch {
    $existingInstance = $null
}

if (-not $existingInstance) {
    $diskInstance = Initialize-AzDataProtectionBackupInstance -DatasourceType AzureDisk -DatasourceLocation (Get-AzResourceGroup -Name $ResourceGroupName).Location -PolicyId $dppPolicy.Id -DatasourceId $diskId -SnapshotResourceGroupId (Get-AzResourceGroup -Name $ResourceGroupName).ResourceId
    $protectedDisk = New-AzDataProtectionBackupInstance -VaultName $dppVaultName -ResourceGroupName $ResourceGroupName -BackupInstance $diskInstance
    Write-Host "Disk protected in DPP vault: $($protectedDisk.Name)" -ForegroundColor Green
    
    # Wait for protection to be configured
    Write-Host "Waiting 60s for DPP protection to stabilize..." -ForegroundColor Yellow
    Start-Sleep -Seconds 60
} else {
    Write-Host "Disk already protected in DPP vault." -ForegroundColor Gray
    $protectedDisk = $existingInstance
}

# Wait for protection status to become ProtectionConfigured
Write-Host "Waiting for DPP protection status to become configured..." -ForegroundColor Yellow
for ($i = 0; $i -lt 12; $i++) {
    $currentInstance = Get-AzDataProtectionBackupInstance -VaultName $dppVaultName -ResourceGroupName $ResourceGroupName | Where-Object {
        $_.Property.DataSourceInfo.ResourceId -eq $diskId
    }
    if ($currentInstance -and $currentInstance.Property.ProtectionStatus.Status -eq "ProtectionConfigured") {
        Write-Host "DPP protection configured." -ForegroundColor Green
        break
    }
    $protectionStatus = if ($currentInstance) { $currentInstance.Property.ProtectionStatus.Status } else { "Not returned yet" }
    Write-Host "  Status: $protectionStatus - waiting 15s..." -ForegroundColor Gray
    Start-Sleep -Seconds 15
}

# Soft-delete the DPP backup instance
Write-Host "Soft-deleting DPP disk backup instance..." -ForegroundColor Yellow
$dppInstanceName = if ($protectedDisk.Name) { $protectedDisk.Name } else { $protectedDisk[0].Name }
try {
    Remove-AzDataProtectionBackupInstance -VaultName $dppVaultName -ResourceGroupName $ResourceGroupName -Name $dppInstanceName
    Write-Host "DPP disk backup instance soft-deleted." -ForegroundColor Green
    Start-Sleep -Seconds 10
} catch {
    Write-Host "DPP soft-delete skipped (may already be deleted): $_" -ForegroundColor Yellow
}

# ── RSV: Enable soft delete, create file share policy, protect file share, then soft-delete ──
Write-Host "Ensuring RSV soft delete is enabled..." -ForegroundColor Yellow
$rsvVault = Get-AzRecoveryServicesVault -Name $rsvVaultName -ResourceGroupName $ResourceGroupName
Set-AzRecoveryServicesVaultContext -Vault $rsvVault

$vaultProperty = Get-AzRecoveryServicesVaultProperty -VaultId $rsvVault.ID
if ($vaultProperty.SoftDeleteFeatureState -ne "Enabled") {
    Set-AzRecoveryServicesVaultProperty -VaultId $rsvVault.ID -SoftDeleteFeatureState Enable
    Write-Host "RSV soft delete enabled." -ForegroundColor Green
} else {
    Write-Host "RSV soft delete already enabled." -ForegroundColor Gray
}

# Register storage account with RSV
Write-Host "Registering storage account with RSV..." -ForegroundColor Yellow
try {
    $container = Get-AzRecoveryServicesBackupContainer -ContainerType AzureStorage -FriendlyName $storageAccountName -VaultId $rsvVault.ID -ErrorAction SilentlyContinue
    if (-not $container) {
        Register-AzRecoveryServicesBackupContainer -ResourceId $storageAccountId -VaultId $rsvVault.ID -BackupManagementType AzureStorage -Force
        Write-Host "Storage account registered." -ForegroundColor Green
        Start-Sleep -Seconds 15
    } else {
        Write-Host "Storage account already registered." -ForegroundColor Gray
    }
} catch {
    Write-Host "Storage account registration: $_" -ForegroundColor Yellow
}

# Create or get file share protection policy
$rsvPolicyName = "undelete-fs-policy"
Write-Host "Creating RSV file share backup policy..." -ForegroundColor Yellow
$rsvPolicy = Get-AzRecoveryServicesBackupProtectionPolicy -VaultId $rsvVault.ID -Name $rsvPolicyName -ErrorAction SilentlyContinue
if (-not $rsvPolicy) {
    $schedulePolicy = Get-AzRecoveryServicesBackupSchedulePolicyObject -WorkloadType AzureFiles -ScheduleRunFrequency Daily
    $retentionPolicy = Get-AzRecoveryServicesBackupRetentionPolicyObject -WorkloadType AzureFiles
    $rsvPolicy = New-AzRecoveryServicesBackupProtectionPolicy -VaultId $rsvVault.ID -Name $rsvPolicyName -WorkloadType AzureFiles -RetentionPolicy $retentionPolicy -SchedulePolicy $schedulePolicy
    Write-Host "RSV file share policy created." -ForegroundColor Green
} else {
    Write-Host "RSV file share policy already exists." -ForegroundColor Gray
}

# Protect the file share
Write-Host "Protecting file share in RSV..." -ForegroundColor Yellow
$existingFsItem = Get-AzRecoveryServicesBackupItem -VaultId $rsvVault.ID -BackupManagementType AzureStorage -WorkloadType AzureFiles -ErrorAction SilentlyContinue | Where-Object {
    $_.FriendlyName -eq $fileShareName
}

if (-not $existingFsItem) {
    Enable-AzRecoveryServicesBackupProtection -VaultId $rsvVault.ID -Policy $rsvPolicy -StorageAccountName $storageAccountName -Name $fileShareName
    Write-Host "File share protected in RSV." -ForegroundColor Green
    Start-Sleep -Seconds 30
} else {
    Write-Host "File share already protected in RSV." -ForegroundColor Gray
}

# Soft-delete the RSV file share backup
Write-Host "Soft-deleting RSV file share backup..." -ForegroundColor Yellow
$protectedFs = Get-AzRecoveryServicesBackupItem -VaultId $rsvVault.ID -BackupManagementType AzureStorage -WorkloadType AzureFiles -ErrorAction SilentlyContinue | Where-Object {
    $_.FriendlyName -eq $fileShareName
}
if ($protectedFs) {
    try {
        Disable-AzRecoveryServicesBackupProtection -Item $protectedFs -VaultId $rsvVault.ID -RemoveRecoveryPoints -Force
        Write-Host "RSV file share backup soft-deleted." -ForegroundColor Green
        Start-Sleep -Seconds 10
    } catch {
        Write-Host "RSV soft-delete skipped: $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "No protected file share found to soft-delete." -ForegroundColor Yellow
}

Write-Host "Undelete test infrastructure setup complete." -ForegroundColor Cyan
