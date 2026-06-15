// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public class VaultCreateOptions : BaseAzureBackupOptions
{
    [Option(AzureBackupOptionDefinitions.Location)]
    public required string Location { get; set; }

    [Option("The vault SKU.")]
    public string? Sku { get; set; }

    [Option("Storage redundancy: 'GeoRedundant', 'LocallyRedundant', or 'ZoneRedundant'.")]
    public string? StorageType { get; set; }
}
