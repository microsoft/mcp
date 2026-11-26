// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.RegularExpressions;

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Makes Azure Portal-relative Markdown links usable outside the Azure Portal host without changing
/// any other response content.
/// </summary>
internal static class AzurePortalMarkdownLinkNormalizer
{
    private const string PortalOrigin = "https://portal.azure.com";
    private static readonly Regex s_markdownLink = new(
        @"(?<!!)\[(?<text>(?:\\[^\r\n]|[^\[\]\\\r\n])*)\]\((?<target><[^<>\r\n]*>|(?:[^\s()<]|\([^\s()]*\))+)\)",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    internal static string? Normalize(string? markdown)
    {
        if (string.IsNullOrEmpty(markdown)
            || !markdown.Contains("](", StringComparison.Ordinal))
        {
            return markdown;
        }

        var result = new StringBuilder(markdown.Length);
        char fenceMarker = '\0';
        int fenceLength = 0;
        int lineStart = 0;

        while (lineStart < markdown.Length)
        {
            int newline = markdown.IndexOf('\n', lineStart);
            int contentEnd = newline >= 0 ? newline : markdown.Length;
            int logicalEnd = contentEnd > lineStart && markdown[contentEnd - 1] == '\r'
                ? contentEnd - 1
                : contentEnd;
            string line = markdown[lineStart..logicalEnd];
            bool inFence = fenceMarker != '\0';
            bool fenceLine = TryReadFence(line, out char marker, out int markerLength, out int suffixStart);

            if (!inFence && fenceLine)
            {
                fenceMarker = marker;
                fenceLength = markerLength;
                result.Append(line);
            }
            else if (inFence)
            {
                result.Append(line);
                if (fenceLine
                    && marker == fenceMarker
                    && markerLength >= fenceLength
                    && IsWhitespace(line, suffixStart))
                {
                    fenceMarker = '\0';
                    fenceLength = 0;
                }
            }
            else if (IsIndentedCode(line))
            {
                result.Append(line);
            }
            else
            {
                result.Append(NormalizeOutsideInlineCode(line));
            }

            if (newline >= 0)
            {
                result.Append(markdown, logicalEnd, newline + 1 - logicalEnd);
                lineStart = newline + 1;
            }
            else
            {
                lineStart = markdown.Length;
            }
        }

        return result.ToString();
    }

    private static string NormalizeOutsideInlineCode(string line)
    {
        if (!line.Contains("](", StringComparison.Ordinal))
        {
            return line;
        }

        var result = new StringBuilder(line.Length);
        int start = 0;
        while (start < line.Length)
        {
            int codeStart = line.IndexOf('`', start);
            if (codeStart < 0)
            {
                AppendNormalizedLinks(result, line[start..]);
                break;
            }

            AppendNormalizedLinks(result, line[start..codeStart]);
            int delimiterLength = CountRun(line, codeStart, '`');
            int codeEnd = FindMatchingRun(line, codeStart + delimiterLength, '`', delimiterLength);
            if (codeEnd < 0)
            {
                result.Append(line, codeStart, line.Length - codeStart);
                break;
            }

            int end = codeEnd + delimiterLength;
            result.Append(line, codeStart, end - codeStart);
            start = end;
        }

        return result.ToString();
    }

    private static void AppendNormalizedLinks(StringBuilder result, string text)
    {
        result.Append(s_markdownLink.Replace(text, match =>
        {
            string target = match.Groups["target"].Value;
            bool angleDelimited = target.Length >= 2 && target[0] == '<' && target[^1] == '>';
            string destination = angleDelimited ? target[1..^1] : target;
            if (!IsPortalFragment(destination))
            {
                return match.Value;
            }

            string absolute = $"{PortalOrigin}/{destination}";
            string normalizedTarget = angleDelimited ? $"<{absolute}>" : absolute;
            return $"[{match.Groups["text"].Value}]({normalizedTarget})";
        }));
    }

    private static bool IsPortalFragment(string destination)
    {
        if (destination.StartsWith("#view/", StringComparison.OrdinalIgnoreCase)
            || destination.StartsWith("#blade/", StringComparison.OrdinalIgnoreCase)
            || destination.StartsWith("#resource/", StringComparison.OrdinalIgnoreCase)
            || IsResourceRoute(destination, "#@/resource"))
        {
            return true;
        }

        if (!destination.StartsWith("#@", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        int tenantEnd = destination.IndexOf('/', 2);
        if (tenantEnd <= 2)
        {
            return false;
        }

        ReadOnlySpan<char> tenant = destination.AsSpan(2, tenantEnd - 2);
        if (tenant.IsEmpty
            || tenant.IndexOfAnyExcept(
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-") >= 0)
        {
            return false;
        }

        ReadOnlySpan<char> route = destination.AsSpan(tenantEnd + 1);
        return route.Equals("resource", StringComparison.OrdinalIgnoreCase)
            || route.StartsWith("resource/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsResourceRoute(string destination, string prefix) =>
        destination.Equals(prefix, StringComparison.OrdinalIgnoreCase)
        || destination.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);

    private static bool TryReadFence(
        string line,
        out char marker,
        out int markerLength,
        out int suffixStart)
    {
        marker = '\0';
        markerLength = 0;
        suffixStart = 0;
        int index = 0;

        while (true)
        {
            int spaces = 0;
            while (index < line.Length && spaces < 3 && line[index] == ' ')
            {
                index++;
                spaces++;
            }

            if (index >= line.Length || line[index] != '>')
            {
                break;
            }

            index++;
            if (index < line.Length && line[index] == ' ')
            {
                index++;
            }
        }

        if (index >= line.Length || line[index] is not ('`' or '~'))
        {
            return false;
        }

        marker = line[index];
        markerLength = CountRun(line, index, marker);
        suffixStart = index + markerLength;
        return markerLength >= 3;
    }

    private static bool IsIndentedCode(string line) =>
        line.StartsWith('\t')
        || line.StartsWith("    ", StringComparison.Ordinal);

    private static bool IsWhitespace(string value, int start)
    {
        for (int index = start; index < value.Length; index++)
        {
            if (!char.IsWhiteSpace(value[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static int CountRun(string value, int start, char character)
    {
        int index = start;
        while (index < value.Length && value[index] == character)
        {
            index++;
        }

        return index - start;
    }

    private static int FindMatchingRun(
        string value,
        int start,
        char character,
        int expectedLength)
    {
        int index = start;
        while (index < value.Length)
        {
            if (value[index] != character)
            {
                index++;
                continue;
            }

            int length = CountRun(value, index, character);
            if (length == expectedLength)
            {
                return index;
            }

            index += length;
        }

        return -1;
    }
}
