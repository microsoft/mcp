// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

public static class VaultTypeResolver
{
    public const string Rsv = "rsv";
    public const string Dpp = "dpp";

    public static bool IsRsv(string? vaultType) =>
        string.Equals(vaultType, Rsv, StringComparison.OrdinalIgnoreCase);

    public static bool IsDpp(string? vaultType) =>
        string.Equals(vaultType, Dpp, StringComparison.OrdinalIgnoreCase);

    public static void ValidateVaultType(string? vaultType)
    {
        if (string.IsNullOrEmpty(vaultType))
        {
            throw new ArgumentException("The --vault-type parameter is required. Specify 'rsv' for Recovery Services vault or 'dpp' for Backup vault.");
        }

        if (!IsRsv(vaultType) && !IsDpp(vaultType))
        {
            throw new ArgumentException($"Invalid vault type '{vaultType}'. Must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }
    }

    public static bool IsVaultTypeSpecified(string? vaultType)
    {
        if (string.IsNullOrEmpty(vaultType))
        {
            return false;
        }

        if (!IsRsv(vaultType) && !IsDpp(vaultType))
        {
            throw new ArgumentException($"Invalid vault type '{vaultType}'. Must be 'rsv' (Recovery Services vault) or 'dpp' (Backup vault).");
        }

        return true;
    }
}
