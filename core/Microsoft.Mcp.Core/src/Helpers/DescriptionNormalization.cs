// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace Microsoft.Mcp.Core.Helpers;

/// <summary>
/// Normalizes command description text before it is advertised to MCP clients and CLI help.
/// </summary>
internal static class DescriptionNormalization
{
    /// <summary>
    /// Joins soft-wrapped lines so multi-line source literals do not spend tokens on line breaks.
    /// Line endings become '\n', blank lines collapse to a single paragraph break, and lines that
    /// start a list item ("- ", "* ", "1. "), end with ':', or follow a line ending with ':' keep their own line.
    /// </summary>
    /// <param name="description">The description as written in source.</param>
    /// <returns>The normalized description.</returns>
    public static string Normalize(string description)
    {
        if (string.IsNullOrEmpty(description) || description.IndexOfAny(['\r', '\n']) < 0)
        {
            return description;
        }

        var builder = new StringBuilder(description.Length);
        var paragraphBreak = false;
        var previousEndsWithColon = false;

        foreach (var rawLine in description.ReplaceLineEndings("\n").Split('\n'))
        {
            var line = rawLine.TrimEnd();
            if (line.Length == 0)
            {
                paragraphBreak = builder.Length > 0;
                continue;
            }

            if (builder.Length == 0)
            {
                builder.Append(line);
            }
            else if (paragraphBreak || previousEndsWithColon || IsListItem(line) || line.EndsWith(':'))
            {
                builder.Append('\n').Append(line);
            }
            else
            {
                builder.Append(' ').Append(line.TrimStart());
            }

            paragraphBreak = false;
            previousEndsWithColon = line.EndsWith(':');
        }

        return builder.ToString();
    }

    private static bool IsListItem(string line)
    {
        var text = line.AsSpan().TrimStart();
        if (text.Length >= 2 && text[0] is '-' or '*' or '\u2022' && text[1] == ' ')
        {
            return true;
        }

        var digits = 0;
        while (digits < text.Length && char.IsAsciiDigit(text[digits]))
        {
            digits++;
        }

        return digits > 0
            && digits + 1 < text.Length
            && text[digits] is '.' or ')'
            && text[digits + 1] == ' ';
    }
}
