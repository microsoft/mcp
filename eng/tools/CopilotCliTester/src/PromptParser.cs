// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using CopilotCliTester.Models;

namespace CopilotCliTester;

/// <summary>
/// Parses test prompts from e2eTestPrompts.md
/// </summary>
internal static partial class PromptParser
{
    [GeneratedRegex(@"^## (.+)$")]
    private static partial Regex SectionHeaderRegex();

    [GeneratedRegex(@"^\|\s*([a-z0-9_-]+)\s*\|\s*(.+)\s*\|$", RegexOptions.IgnoreCase)]
    private static partial Regex TableRowRegex();

    public static List<string> ParseNamespaces(string filePath)
    {
        var namespaces = new List<string>();
        foreach (var line in File.ReadLines(filePath))
        {
            var match = SectionHeaderRegex().Match(line);
            if (match.Success)
            {
                namespaces.Add(match.Groups[1].Value);
            }
        }
        return namespaces;
    }

    public static List<TestPrompt> ParseFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        var prompts = new List<TestPrompt>();
        var currentSection = string.Empty;

        foreach (var line in lines)
        {
            var sectionMatch = SectionHeaderRegex().Match(line);
            if (sectionMatch.Success)
            {
                currentSection = sectionMatch.Groups[1].Value;
                continue;
            }

            // Parse table rows
            var rowMatch = TableRowRegex().Match(line);
            if (rowMatch.Success)
            {
                var tool = rowMatch.Groups[1].Value.Trim();
                var prompt = rowMatch.Groups[2].Value.Trim();

                // Skip header rows
                if (tool.Equals("Tool Name", StringComparison.OrdinalIgnoreCase) ||
                    tool.All(c => c == '-'))
                {
                    continue;
                }

                prompts.Add(new TestPrompt(currentSection, tool, prompt, currentSection));
            }
        }

        return prompts;
    }

}
