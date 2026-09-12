// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>
/// Describes the selective disk backup configuration for an IaaS VM protected item.
/// Corresponds to the Azure Backup REST contract
/// <c>IaasVmProtectedItem.extendedProperties.diskExclusionProperties</c>
/// (see https://learn.microsoft.com/azure/backup/selective-disk-backup-restore).
/// </summary>
/// <param name="Setting">
/// Case-insensitive selection mode:
/// <c>include</c> (back up only the LUNs in <see cref="DiskLunsCsv"/>),
/// <c>exclude</c> (back up all disks except the LUNs in <see cref="DiskLunsCsv"/>), or
/// <c>resetexclusionsettings</c> (clear all selective disk configuration).
/// </param>
/// <param name="DiskLunsCsv">Comma-separated data disk LUNs (non-negative integers).</param>
/// <param name="ExcludeAllDataDisks">When true, only the OS disk is protected. Overrides <see cref="DiskLunsCsv"/>.</param>
public sealed record DiskExclusionSpec(
    AzureBackupDiskListSetting? Setting,
    string? DiskLunsCsv,
    bool ExcludeAllDataDisks)
{
    /// <summary>Returns true when at least one selective disk option was provided by the caller.</summary>
    public bool HasAnyValue =>
        Setting is not null ||
        !string.IsNullOrWhiteSpace(DiskLunsCsv) ||
        ExcludeAllDataDisks;
}
