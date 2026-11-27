// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ResiliencyAgent.Services;

/// <summary>
/// Shared Azure Portal URL and Markdown escaping helpers for agent-authored reports.
/// </summary>
internal static class MarkdownRenderingHelpers
{
    private const string PortalOrigin = "https://portal.azure.com";

    internal static string AbsolutePortalLink(string link) =>
        link.StartsWith('#')
            ? $"{PortalOrigin}/{link}"
            : link;

    internal static string BuildPortalResourceUrl(string resourceId)
    {
        string[] segments = resourceId
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 4
            && string.Equals(segments[0], "providers", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[1], "Microsoft.Management", StringComparison.OrdinalIgnoreCase)
            && string.Equals(segments[2], "serviceGroups", StringComparison.OrdinalIgnoreCase))
        {
            if (Guid.TryParse(segments[3], out _))
            {
                string encodedResourceId = Uri.EscapeDataString("/" + string.Join("/", segments));
                return $"{PortalOrigin}/#view/Microsoft_Azure_Resources/ServiceGroupsBrowse.ReactView/" +
                    $"id/{encodedResourceId}";
            }

            return $"{PortalOrigin}/#view/Microsoft_Azure_Resources/ServiceGroup.MenuView/~/overview/" +
                $"serviceGroupId/{Uri.EscapeDataString(segments[3])}";
        }

        return $"{PortalOrigin}/#@/resource/{string.Join("/", segments.Select(Uri.EscapeDataString))}";
    }

    internal static string EscapeTableCell(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("`", "\\`", StringComparison.Ordinal)
            .Replace("*", "\\*", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal)
            .Replace("[", "\\[", StringComparison.Ordinal)
            .Replace("]", "\\]", StringComparison.Ordinal)
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal);

    internal static string EscapeLinkLabel(string value) =>
        EscapeTableCell(value)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);
}
