// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Tools.AzureBackup.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.RecoveryServices;
using Azure.ResourceManager.RecoveryServicesBackup;
using Azure.ResourceManager.RecoveryServicesBackup.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Services;

// Selective Disk Backup and Restore support for IaaS VM protected items.
// See https://learn.microsoft.com/azure/backup/selective-disk-backup-restore
// Implements 'update-protection' and the shared disk-exclusion translation used
// by both 'protect' and 'update-protection'.
public sealed partial class RsvBackupOperations
{
    public async Task<ProtectResult> UpdateProtectionAsync(
        string vaultName, string resourceGroup, string subscription,
        string datasourceId, string? policyName, DiskExclusionSpec? diskExclusion,
        string? containerName, string? tenant, CancellationToken cancellationToken)
    {
        ValidateRequiredParameters(
            (nameof(vaultName), vaultName),
            (nameof(resourceGroup), resourceGroup),
            (nameof(subscription), subscription),
            (nameof(datasourceId), datasourceId));

        var hasDiskExclusion = diskExclusion is not null && diskExclusion.HasAnyValue;
        var hasPolicyChange = !string.IsNullOrWhiteSpace(policyName);
        if (!hasDiskExclusion && !hasPolicyChange)
        {
            throw new ArgumentException(
                "At least one of --policy, --disk-list-setting, --disks-list, or --exclude-all-data-disks must be provided for 'protecteditem update-protection'.");
        }

        // 'update-protection' is a VM-only operation (mirrors 'az backup protection update-for-vm').
        // The datasource-id must be a Compute VM ARM ID.
        if (!datasourceId.Contains("/virtualMachines/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "The 'protecteditem update-protection' command is only supported for IaaS VM protected items. " +
                "Pass the VM ARM resource ID (e.g., '/subscriptions/.../virtualMachines/my-vm') as --datasource-id.");
        }

        var armClient = await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);

        var vaultId = RecoveryServicesVaultResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName);
        var vaultResource = armClient.GetRecoveryServicesVaultResource(vaultId);
        var vault = await vaultResource.GetAsync(cancellationToken: cancellationToken);
        var vaultLocation = vault.Value.Data.Location;

        var container = containerName ?? RsvNamingHelper.DeriveContainerName(datasourceId);
        var vmProtectedItemName = RsvNamingHelper.DeriveProtectedItemName(datasourceId);

        var vmProtectedItemId = BackupProtectedItemResource.CreateResourceIdentifier(
            subscription, resourceGroup, vaultName, FabricName, container, vmProtectedItemName);
        var vmProtectedItemResource = armClient.GetBackupProtectedItemResource(vmProtectedItemId);

        // Fetch the existing protected item so we can preserve non-updated fields.
        var existing = await vmProtectedItemResource.GetAsync(cancellationToken: cancellationToken);
        if (existing.Value.Data.Properties is not IaasComputeVmProtectedItem existingVm)
        {
            throw new InvalidOperationException(
                $"Protected item '{vmProtectedItemName}' is not an IaaS VM protected item. 'update-protection' only supports IaaS VM backups.");
        }

        var updatedPolicyId = hasPolicyChange
            ? BackupProtectionPolicyResource.CreateResourceIdentifier(subscription, resourceGroup, vaultName, policyName!)
            : existingVm.PolicyId;

        var updatedVm = new IaasComputeVmProtectedItem
        {
            PolicyId = updatedPolicyId,
            SourceResourceId = existingVm.SourceResourceId
        };

        // Preserve any existing extended properties (e.g. disk exclusion) when the caller
        // did not explicitly change disk settings, so a pure policy update is safe.
        if (hasDiskExclusion)
        {
            ApplyDiskExclusionToProtectedItem(updatedVm, diskExclusion);
        }
        else if (existingVm.ExtendedProperties is not null)
        {
            updatedVm.ExtendedProperties = existingVm.ExtendedProperties;
        }

        var vmProtectedItemData = new BackupProtectedItemData(vaultLocation)
        {
            Properties = updatedVm
        };

        var updateResult = await vmProtectedItemResource.UpdateAsync(WaitUntil.Started, vmProtectedItemData, cancellationToken);

        var jobId = await FindLatestJobIdAsync(armClient, subscription, resourceGroup, vaultName, "ConfigureBackup", cancellationToken);
        jobId ??= ExtractOperationIdFromResponse(updateResult.GetRawResponse());

        return await BuildRsvProtectResultAsync(
            armClient, subscription, resourceGroup, vaultName, vmProtectedItemName, jobId,
            "VM protection update", cancellationToken);
    }

    /// <summary>
    /// Translates a <see cref="DiskExclusionSpec"/> into the SDK
    /// <c>IaasVmBackupExtendedProperties.DiskExclusionProperties</c> shape and applies it to
    /// the given IaaS VM protected item. No-ops when the spec is null or empty.
    /// Mirrors the CLI contract of <c>az backup protection enable-for-vm --disk-list-setting</c>.
    /// </summary>
    internal static void ApplyDiskExclusionToProtectedItem(IaasComputeVmProtectedItem vmProtectedItem, DiskExclusionSpec? diskExclusion)
    {
        if (diskExclusion is null || !diskExclusion.HasAnyValue)
        {
            return;
        }

        var setting = diskExclusion.Setting?.Trim();

        // resetexclusionsettings clears any prior selective-disk configuration and backs up all disks.
        if (string.Equals(setting, DiskExclusionSpec.SettingReset, StringComparison.OrdinalIgnoreCase))
        {
            vmProtectedItem.ExtendedProperties = new IaasVmBackupExtendedProperties
            {
                DiskExclusionProperties = null
            };
            return;
        }

        // --exclude-all-data-disks: back up only the OS disk. Represented as an empty
        // "include list" (i.e. include nothing beyond the OS disk).
        //
        // Serialization NOTE: `DiskLunList` is a `ChangeTrackingList<int>` — when it is never
        // touched, the SDK omits it from the JSON payload, and the RSV service rejects
        // `{IsInclusionList: true}` alone with "Provided disk exclusion setting is invalid...
        // no list is provided.". We touch the list (Add + Clear) to force the SDK to
        // serialize an explicit empty `diskLunList: []`, which the service accepts.
        if (diskExclusion.ExcludeAllDataDisks)
        {
            var excludeAllProps = new DiskExclusionProperties
            {
                IsInclusionList = true,
            };
            excludeAllProps.DiskLunList.Add(0);
            excludeAllProps.DiskLunList.Clear();
            vmProtectedItem.ExtendedProperties = new IaasVmBackupExtendedProperties
            {
                DiskExclusionProperties = excludeAllProps
            };
            return;
        }

        // include / exclude paths must specify the LUN list explicitly.
        var isInclusionList = string.Equals(setting, DiskExclusionSpec.SettingInclude, StringComparison.OrdinalIgnoreCase);
        var luns = ParseDiskLuns(diskExclusion.DiskLunsCsv);

        var props = new DiskExclusionProperties { IsInclusionList = isInclusionList };
        foreach (var lun in luns)
        {
            props.DiskLunList.Add(lun);
        }

        vmProtectedItem.ExtendedProperties = new IaasVmBackupExtendedProperties
        {
            DiskExclusionProperties = props
        };
    }

    private static IReadOnlyList<int> ParseDiskLuns(string? diskLunsCsv)
    {
        if (string.IsNullOrWhiteSpace(diskLunsCsv))
        {
            throw new ArgumentException(
                "--disks-list is required when --disk-list-setting is 'include' or 'exclude'. " +
                "Provide a comma-separated list of non-negative data disk LUNs (e.g. '0,1,3').");
        }

        var luns = new List<int>();
        foreach (var raw in diskLunsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!int.TryParse(raw, out var lun) || lun < 0)
            {
                throw new ArgumentException(
                    $"Invalid disk LUN '{raw}' in --disks-list. Data disk LUNs must be non-negative integers (e.g. '0,1,3').");
            }

            luns.Add(lun);
        }

        if (luns.Count == 0)
        {
            throw new ArgumentException(
                "--disks-list must contain at least one non-negative data disk LUN (e.g. '0,1,3').");
        }

        return luns;
    }
}
