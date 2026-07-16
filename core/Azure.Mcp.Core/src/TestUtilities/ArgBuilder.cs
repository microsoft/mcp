// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.TestUtilities;

public static class ArgBuilder
{
    // Build an args array, omitting any option whose name matches missingOption
    public static string[] BuildArgs(string? missingOption, params (string name, string value)[] pairs)
    {
        var list = new List<string>();
        foreach (var (name, value) in pairs)
        {
            if (missingOption != null && name == missingOption)
                continue;
            list.Add(name);
            list.Add(value);
        }
        return [.. list];
    }
}
