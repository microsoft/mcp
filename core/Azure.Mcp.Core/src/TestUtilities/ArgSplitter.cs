// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Azure.Mcp.TestUtilities;

public static class ArgSplitter
{
    // Split command-line strings into arguments while respecting double quotes
    public static string[] SplitArgs(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine))
            return [];

        var args = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < commandLine.Length; i++)
        {
            var c = commandLine[i];
            if (c == '"')
            {
                // Determine if this quote is escaped by an odd number of backslashes
                int backslashCount = 0;
                for (int j = i - 1; j >= 0 && commandLine[j] == '\\'; j--)
                    backslashCount++;

                if (backslashCount % 2 == 1)
                {
                    // Escaped quote: remove one escaping backslash already appended (if any)
                    if (current.Length > 0 && current[current.Length - 1] == '\\')
                        current.Length--;
                    current.Append('"');
                    continue;
                }

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

        return [.. args];
    }
}
