// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public class VaultCreateOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.SkuName)]
    public string? Sku { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.StorageTypeName)]
    public string? StorageType { get; set; }
}
