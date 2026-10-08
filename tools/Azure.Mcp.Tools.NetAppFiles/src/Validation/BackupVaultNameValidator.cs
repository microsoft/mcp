// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class BackupVaultNameValidator
{
    public const string ErrorMessage = "--backup-vault must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.";

    public static bool IsValid(string name)
    {
        if (name.Length is < 1 or > 64 || !char.IsAsciiLetterOrDigit(name[0]))
        {
            return false;
        }

        foreach (var character in name)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-')
            {
                return false;
            }
        }

        return true;
    }
}
