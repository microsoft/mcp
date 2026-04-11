// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Vault;

public class VaultUpdateOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.SoftDeleteName)]
    public string? SoftDeleteState { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.SoftDeleteRetentionDaysName)]
    public string? SoftDeleteRetentionDays { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.ImmutabilityStateName)]
    public string? ImmutabilityState { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.IdentityTypeName)]
    public string? IdentityType { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.TagsName)]
    public string? Tags { get; set; }
}
