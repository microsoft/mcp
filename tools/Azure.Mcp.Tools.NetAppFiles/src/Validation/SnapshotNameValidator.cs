// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class SnapshotNameValidator
{
    public const string ErrorMessage = "--snapshot must be 1-255 characters, start with an alphanumeric character, and contain only alphanumerics, underscores, and hyphens.";

    public static bool IsValid(string name)
    {
        if (name.Length is < 1 or > 255 || !char.IsAsciiLetterOrDigit(name[0]))
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