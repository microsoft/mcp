// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public sealed class VaultUpdateOptions : BaseAzureBackupOptions
{
    [Option(Description = "Storage redundancy: 'GeoRedundant', 'LocallyRedundant', 'ZoneRedundant', or 'ReadAccessGeoZoneRedundant'.")]
    public string? Redundancy { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.SoftDelete)]
    public string? SoftDelete { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.SoftDeleteRetentionDays)]
    public string? SoftDeleteRetentionDays { get; set; }

    [Option(Description = AzureBackupOptionDefinitions.ImmutabilityState)]
    public string? ImmutabilityState { get; set; }

    [Option(Description = "Managed identity type: 'SystemAssigned', 'UserAssigned', 'SystemAssigned,UserAssigned', or 'None'.")]
    public string? IdentityType { get; set; }

    [Option(Description = "Resource tags as JSON key-value object.")]
    public string? Tags { get; set; }
}
