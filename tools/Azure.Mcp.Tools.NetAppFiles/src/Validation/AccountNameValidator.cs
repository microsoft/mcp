// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class AccountNameValidator
{
    public const string ErrorMessage = "--account must be 1-128 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.";

    public static bool IsValid(string account)
    {
        if (account.Length is < 1 or > 128 || !char.IsAsciiLetterOrDigit(account[0]))
        {
            return false;
        }

        foreach (var character in account)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-')
            {
                return false;
            }
        }

        return true;
    }
}
