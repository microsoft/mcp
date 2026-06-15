// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Security;

public class SecurityConfigureEncryptionOptions : BaseAzureBackupOptions
{
    [Option("Key Vault URI (e.g., 'https://kv-security-prod.vault.azure.net/').")]
    public required string KeyVaultUri { get; set; }

    [Option("Name of the encryption key in the Key Vault.")]
    public required string KeyName { get; set; }

    [Option("Specific key version. Omit to always use the latest version.")]
    public string? KeyVersion { get; set; }

    [Option("Managed identity type: 'SystemAssigned', 'UserAssigned', 'SystemAssigned,UserAssigned', or 'None'.")]
    public required string IdentityType { get; set; }

    [Option("ARM resource ID of the user-assigned managed identity for Key Vault access. Required when --identity-type is 'UserAssigned'.")]
    public string? UserAssignedIdentityId { get; set; }
}
