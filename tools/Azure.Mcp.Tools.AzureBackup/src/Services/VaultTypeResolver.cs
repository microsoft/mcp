// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureBackup.Models;

namespace Azure.Mcp.Tools.AzureBackup.Services;

public static class VaultTypeResolver
{
    public const string Rsv = "rsv";
    public const string Dpp = "dpp";

    public static bool IsRsv(AzureBackupVaultType? vaultType) =>
        vaultType == AzureBackupVaultType.Rsv;

    public static bool IsDpp(AzureBackupVaultType? vaultType) =>
        vaultType == AzureBackupVaultType.Dpp;

    public static void ValidateVaultType(AzureBackupVaultType? vaultType)
    {
        if (vaultType is null)
        {
            throw new ArgumentException("The --vault-type parameter is required. Specify 'rsv' for Recovery Services vault or 'dpp' for Backup vault.");
        }

        if (!Enum.IsDefined(vaultType.Value))
        {
            throw new ArgumentException($"Invalid vault type '{vaultType}'. Must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }
    }

    public static bool IsVaultTypeSpecified(AzureBackupVaultType? vaultType)
    {
        if (vaultType is null)
        {
            return false;
        }

        if (!Enum.IsDefined(vaultType.Value))
        {
            throw new ArgumentException($"Invalid vault type '{vaultType}'. Must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }

        return true;
    }
}
