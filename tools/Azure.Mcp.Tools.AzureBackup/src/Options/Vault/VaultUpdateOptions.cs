// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public class VaultUpdateOptions : BaseAzureBackupOptions
{
    [Option("Storage redundancy: 'GeoRedundant', 'LocallyRedundant', 'ZoneRedundant', or 'ReadAccessGeoZoneRedundant'.")]
    public string? Redundancy { get; set; }

    [Option(AzureBackupOptionDefinitions.SoftDelete)]
    public string? SoftDelete { get; set; }

    [Option(AzureBackupOptionDefinitions.SoftDeleteRetentionDays)]
    public string? SoftDeleteRetentionDays { get; set; }

    [Option(AzureBackupOptionDefinitions.ImmutabilityState)]
    public string? ImmutabilityState { get; set; }

    [Option("Managed identity type: 'SystemAssigned', 'UserAssigned', 'SystemAssigned,UserAssigned', or 'None'.")]
    public string? IdentityType { get; set; }

    [Option("Resource tags as JSON key-value object.")]
    public string? Tags { get; set; }
}
