#!/usr/bin/env pwsh

# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License.

<#
.SYNOPSIS
Pre-removal cleanup for Azure Backup test resources.

.DESCRIPTION
Azure Backup vaults (Recovery Services Vaults and Data Protection Backup Vaults)
cannot be deleted while they still contain backup instances, protected items,
registered containers, or backup policies. In addition, soft-deleted items are
retained inside the vault and block deletion until they are undeleted and hard-deleted.

This script removes those dependencies in the required order so that the resource
group cleanup performed by Remove-TestResources.ps1 succeeds without manual
intervention. It is idempotent: it is safe to re-run and it exits cleanly when
there is nothing left to clean up.

Required cleanup sequence (per resource group):

    1. Disable soft delete on each vault.
    2. Undelete + delete all Backup Instances / Protected Items.
    3. Unregister all Backup Containers (RSV only).
    4. Delete all Backup Policies.
    5. Return control so that Remove-TestResources.ps1 can delete the vault and RG.

The script tolerates missing modules, missing vaults, and per-item failures — it
logs a warning and keeps going so that as much cleanup as possible is completed.

.PARAMETER ResourceGroupName
The resource group containing the Azure Backup test vaults.
#>

[CmdletBinding(SupportsShouldProcess = $true)]
param (
    [Parameter(Mandatory = $true)]
    [string] $ResourceGroupName,

    # Captures any arguments from Remove-TestResources.ps1
    [Parameter(ValueFromRemainingArguments = $true)]
    $RemainingArguments
)

$ErrorActionPreference = 'Continue'

# Best-effort import of the modules we need. These are already available on the
# Azure DevOps hosted agents and are pulled in by the test-resources-post.ps1
# script during deployment, so a missing module here is unexpected but tolerated.
foreach ($module in @('Az.Resources', 'Az.RecoveryServices', 'Az.DataProtection')) {
    if (-not (Get-Module -Name $module)) {
        Import-Module -Name $module -ErrorAction SilentlyContinue
    }
}

function Write-Info([string] $message) {
    Write-Host "[AzureBackup cleanup] $message" -ForegroundColor Cyan
}

function Write-Skip([string] $message) {
    Write-Host "[AzureBackup cleanup] $message" -ForegroundColor Gray
}

function Write-Warn([string] $message) {
    Write-Warning "[AzureBackup cleanup] $message"
}

# ─── Guard: resource group must still exist ──────────────────────────────────
$rg = Get-AzResourceGroup -Name $ResourceGroupName -ErrorAction SilentlyContinue
if (-not $rg) {
    Write-Skip "Resource group '$ResourceGroupName' does not exist. Nothing to clean up."
    return
}

Write-Info "Starting Azure Backup vault cleanup in resource group '$ResourceGroupName'."

# ─── Recovery Services Vaults (RSV) ──────────────────────────────────────────
try {
    $rsvVaults = @(Get-AzRecoveryServicesVault -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue)
} catch {
    Write-Warn "Unable to enumerate Recovery Services Vaults: $($_.Exception.Message)"
    $rsvVaults = @()
}

foreach ($vault in $rsvVaults) {
    Write-Info "Processing Recovery Services Vault '$($vault.Name)'."

    # 1. Disable soft delete so soft-deleted items can be hard-deleted immediately.
    try {
        $vaultProperty = Get-AzRecoveryServicesVaultProperty -VaultId $vault.ID -ErrorAction Stop
        if ($vaultProperty.SoftDeleteFeatureState -ne 'Disabled') {
            Write-Info "  Disabling soft delete on RSV '$($vault.Name)'."
            Set-AzRecoveryServicesVaultProperty -VaultId $vault.ID -SoftDeleteFeatureState 'Disable' -ErrorAction Stop | Out-Null
        } else {
            Write-Skip "  Soft delete already disabled on RSV '$($vault.Name)'."
        }
    } catch {
        Write-Warn "  Failed to disable soft delete on RSV '$($vault.Name)': $($_.Exception.Message)"
    }

    # Enumerate backup items across all backup management types this test infra uses.
    # Additional workload types can be added here without breaking existing calls.
    $backupItemQueries = @(
        @{ BackupManagementType = 'AzureVM';      WorkloadType = 'AzureVM' },
        @{ BackupManagementType = 'AzureStorage'; WorkloadType = 'AzureFiles' },
        @{ BackupManagementType = 'AzureWorkload'; WorkloadType = 'MSSQL' },
        @{ BackupManagementType = 'AzureWorkload'; WorkloadType = 'SAPHanaDatabase' }
    )

    foreach ($query in $backupItemQueries) {
        try {
            $items = @(Get-AzRecoveryServicesBackupItem `
                -VaultId $vault.ID `
                -BackupManagementType $query.BackupManagementType `
                -WorkloadType $query.WorkloadType `
                -ErrorAction SilentlyContinue)
        } catch {
            $items = @()
        }

        foreach ($item in $items) {
            # 2a. Undelete soft-deleted items so they can be hard-deleted.
            if ($item.DeleteState -eq 'ToBeDeleted') {
                try {
                    Write-Info "  Undeleting soft-deleted item '$($item.Name)' in RSV '$($vault.Name)'."
                    Undo-AzRecoveryServicesBackupItemDeletion -Item $item -VaultId $vault.ID -Force -ErrorAction Stop | Out-Null
                } catch {
                    Write-Warn "  Failed to undelete item '$($item.Name)': $($_.Exception.Message)"
                }
            }

            # 2b. Disable protection and remove recovery points (hard delete).
            try {
                Write-Info "  Removing backup for item '$($item.Name)' in RSV '$($vault.Name)'."
                Disable-AzRecoveryServicesBackupProtection -Item $item -VaultId $vault.ID -RemoveRecoveryPoints -Force -ErrorAction Stop | Out-Null
            } catch {
                Write-Warn "  Failed to remove backup for item '$($item.Name)': $($_.Exception.Message)"
            }
        }
    }

    # 3. Unregister backup containers (storage accounts, VMs, etc.) so the vault has no dependencies.
    try {
        $containers = @(Get-AzRecoveryServicesBackupContainer `
            -VaultId $vault.ID `
            -ContainerType AzureStorage `
            -ErrorAction SilentlyContinue)
        foreach ($container in $containers) {
            try {
                Write-Info "  Unregistering storage container '$($container.FriendlyName)' from RSV '$($vault.Name)'."
                Unregister-AzRecoveryServicesBackupContainer -Container $container -VaultId $vault.ID -Force -ErrorAction Stop | Out-Null
            } catch {
                Write-Warn "  Failed to unregister container '$($container.FriendlyName)': $($_.Exception.Message)"
            }
        }
    } catch {
        Write-Warn "  Failed to enumerate storage containers for RSV '$($vault.Name)': $($_.Exception.Message)"
    }

    # 4. Delete backup policies.
    try {
        $policies = @(Get-AzRecoveryServicesBackupProtectionPolicy -VaultId $vault.ID -ErrorAction SilentlyContinue)
    } catch {
        $policies = @()
    }

    foreach ($policy in $policies) {
        try {
            Write-Info "  Removing policy '$($policy.Name)' from RSV '$($vault.Name)'."
            Remove-AzRecoveryServicesBackupProtectionPolicy -Policy $policy -VaultId $vault.ID -Force -ErrorAction Stop | Out-Null
        } catch {
            Write-Warn "  Failed to remove policy '$($policy.Name)': $($_.Exception.Message)"
        }
    }
}

# ─── Data Protection Backup Vaults (DPP) ─────────────────────────────────────
try {
    $dppVaults = @(Get-AzDataProtectionBackupVault -ResourceGroupName $ResourceGroupName -ErrorAction SilentlyContinue)
} catch {
    Write-Warn "Unable to enumerate Data Protection Backup Vaults: $($_.Exception.Message)"
    $dppVaults = @()
}

foreach ($vault in $dppVaults) {
    Write-Info "Processing Data Protection Backup Vault '$($vault.Name)'."

    # 1. Disable soft delete (best-effort; some tenants lock the vault into "AlwaysOn").
    try {
        $currentState = $vault.SoftDeleteState
        if ($currentState -and $currentState -ne 'Off' -and $currentState -ne 'AlwaysOn') {
            Write-Info "  Disabling soft delete on DPP vault '$($vault.Name)' (current state: '$currentState')."
            Update-AzDataProtectionBackupVault `
                -ResourceGroupName $ResourceGroupName `
                -VaultName $vault.Name `
                -SoftDeleteState 'Off' `
                -SoftDeleteRetentionDurationInDay 0 `
                -ErrorAction Stop | Out-Null
        } elseif ($currentState -eq 'AlwaysOn') {
            Write-Skip "  DPP vault '$($vault.Name)' has AlwaysOn soft delete (locked); skipping."
        } else {
            Write-Skip "  Soft delete already disabled on DPP vault '$($vault.Name)'."
        }
    } catch {
        Write-Warn "  Failed to disable soft delete on DPP vault '$($vault.Name)': $($_.Exception.Message)"
    }

    # 2. Delete backup instances (undelete first if they are soft-deleted).
    try {
        $instances = @(Get-AzDataProtectionBackupInstance `
            -ResourceGroupName $ResourceGroupName `
            -VaultName $vault.Name `
            -ErrorAction SilentlyContinue)
    } catch {
        $instances = @()
    }

    foreach ($instance in $instances) {
        $instanceName = $instance.Name

        # 2a. Undo soft delete if the instance is in the SoftDeleted state.
        $isSoftDeleted = $instance.Property.CurrentProtectionState -eq 'SoftDeleted' -or
                         $instance.Property.ProtectionStatus.Status -eq 'SoftDeleted'
        if ($isSoftDeleted) {
            try {
                Write-Info "  Undeleting soft-deleted DPP instance '$instanceName' in vault '$($vault.Name)'."
                Undo-AzDataProtectionBackupInstanceDeletion `
                    -ResourceGroupName $ResourceGroupName `
                    -VaultName $vault.Name `
                    -BackupInstanceName $instanceName `
                    -ErrorAction Stop | Out-Null
            } catch {
                Write-Warn "  Failed to undelete DPP instance '$instanceName': $($_.Exception.Message)"
            }
        }

        # 2b. Hard delete.
        try {
            Write-Info "  Removing DPP instance '$instanceName' from vault '$($vault.Name)'."
            Remove-AzDataProtectionBackupInstance `
                -ResourceGroupName $ResourceGroupName `
                -VaultName $vault.Name `
                -Name $instanceName `
                -ErrorAction Stop | Out-Null
        } catch {
            Write-Warn "  Failed to remove DPP instance '$instanceName': $($_.Exception.Message)"
        }
    }

    # 3. Delete backup policies.
    try {
        $policies = @(Get-AzDataProtectionBackupPolicy `
            -ResourceGroupName $ResourceGroupName `
            -VaultName $vault.Name `
            -ErrorAction SilentlyContinue)
    } catch {
        $policies = @()
    }

    foreach ($policy in $policies) {
        try {
            Write-Info "  Removing DPP policy '$($policy.Name)' from vault '$($vault.Name)'."
            Remove-AzDataProtectionBackupPolicy `
                -ResourceGroupName $ResourceGroupName `
                -VaultName $vault.Name `
                -Name $policy.Name `
                -ErrorAction Stop | Out-Null
        } catch {
            Write-Warn "  Failed to remove DPP policy '$($policy.Name)': $($_.Exception.Message)"
        }
    }
}

Write-Info "Azure Backup vault cleanup complete for resource group '$ResourceGroupName'."
