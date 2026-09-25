// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public sealed class VaultCreateOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Location)]
    public required string Location { get; set; }

    [Option(Description = "The vault SKU.")]
    public string? Sku { get; set; }

    [Option(Description = "Storage redundancy: 'GeoRedundant', 'LocallyRedundant', or 'ZoneRedundant'.")]
    public string? StorageType { get; set; }
}
