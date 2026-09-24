// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Validation;

internal static class NetAppFilesChildResourceNameValidator
{
    public static bool IsValid(string name)
    {
        if (name.Length is < 1 or > 64 || !char.IsAsciiLetterOrDigit(name[0]) || !char.IsAsciiLetterOrDigit(name[^1]))
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