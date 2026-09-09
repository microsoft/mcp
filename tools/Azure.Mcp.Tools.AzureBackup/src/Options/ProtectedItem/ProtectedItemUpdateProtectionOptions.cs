// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ProtectedItem;

public sealed class ProtectedItemUpdateProtectionOptions : BaseProtectedItemOptions
{
    [Option(Description = AzureBackupOptionDefinitions.DatasourceId)]
    public required string DatasourceId { get; set; }

    [Option(Description = "Optional. Name of a new backup policy to attach to the protected item. If omitted, the current policy is retained.")]
    public string? Policy { get; set; }

    // Selective Disk Backup (RSV IaaS VM only) - see https://learn.microsoft.com/azure/backup/selective-disk-backup-restore
    [Option(Description = AzureBackupOptionDefinitions.DiskListSetting)]
    public string? DiskListSetting { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.DisksList)]
    public string? DisksList { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ExcludeAllDataDisks)]
    public bool ExcludeAllDataDisks { get; set; }
}
