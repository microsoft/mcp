// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class BackupNameValidator
{
    public const string BackupErrorMessage = "--backup must be 1-256 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, periods, and hyphens.";
    public const string BackupVaultErrorMessage = "--backup-vault must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.";

    public static bool IsValidBackup(string backup) => IsValid(backup, 256, allowPeriod: true);

    public static bool IsValidBackupVault(string backupVault) => IsValid(backupVault, 64, allowPeriod: false);

    private static bool IsValid(string name, int maximumLength, bool allowPeriod)
    {
        if (name.Length is < 1 || name.Length > maximumLength || !char.IsAsciiLetterOrDigit(name[0]))
        {
            return false;
        }

        foreach (var character in name)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-' && (!allowPeriod || character != '.'))
            {
                return false;
            }
        }

        return true;
    }
}
