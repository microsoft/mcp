// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Security;

public class SecurityConfigureEncryptionOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.KeyVaultUriName)]
    public string? KeyVaultUri { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.KeyNameOptionName)]
    public string? KeyName { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.KeyVersionName)]
    public string? KeyVersion { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.IdentityTypeName)]
    public string? IdentityType { get; set; }

    [JsonPropertyName(AzureBackupOptionDefinitions.UserAssignedIdentityIdName)]
    public string? UserAssignedIdentityId { get; set; }
}
