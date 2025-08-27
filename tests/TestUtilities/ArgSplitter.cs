// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.Mcp.TestUtilities;

public static class ArgSplitter
{
    // Split command-line strings into arguments while respecting double quotes
    public static string[] SplitArgs(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return Array.Empty<string>();

        var args = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            var c = commandLine[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    args.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            args.Add(current.ToString());

        return args.ToArray();
    }
}
