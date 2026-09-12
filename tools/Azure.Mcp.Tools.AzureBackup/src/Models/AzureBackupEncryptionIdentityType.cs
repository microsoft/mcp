// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

/// <summary>Managed identity types supported for customer-managed key encryption.</summary>
public enum AzureBackupEncryptionIdentityType
{
    /// <summary>Use the vault's system-assigned identity.</summary>
    SystemAssigned,

    /// <summary>Use a user-assigned managed identity.</summary>
    UserAssigned
}
