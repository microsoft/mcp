// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Commands;

/// <summary>
/// Vault scope after normalizing a vault name or vault resource ID and a resource group name or resource group resource ID.
/// </summary>
internal sealed record BackupVaultReference(string? ResourceGroup, string? Vault, string? VaultType);
