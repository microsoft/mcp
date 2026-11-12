// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class PoolNameValidator
{
    public const string ErrorMessage = "--pool must be 1-64 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.";

    public static bool IsValid(string pool)
    {
        if (pool.Length is < 1 or > 64 || !char.IsAsciiLetterOrDigit(pool[0]))
        {
            return false;
        }

        foreach (var character in pool)
        {
            if (!char.IsAsciiLetterOrDigit(character) && character is not '_' and not '-')
            {
                return false;
            }
        }

        return true;
    }
}
