// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

internal static class AzureBackupVaultTypeExtensions
{
    internal static string ToValue(this AzureBackupVaultType vaultType) => vaultType switch
    {
        AzureBackupVaultType.Rsv => "rsv",
        AzureBackupVaultType.Dpp => "dpp",
        _ => throw new ArgumentOutOfRangeException(nameof(vaultType), vaultType, null)
    };
}
