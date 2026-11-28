// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Helpers;

/// <summary>
/// Detects an inventory that completed with data-quality warnings (for example missing or invalid CPU, memory or storage) from the
/// Discovery Service operation 'result'. The operation status is 'Succeeded' either way; only the result text, written by the
/// inventory agent, says "Import completed with warnings." and lists the "Data-quality items".
/// </summary>
internal static class InventoryIssues
{
    internal const string WarningsMarker = "completed with warnings";
    internal const string ItemsMarker = "Data-quality items";

    private const int MaxSummaryLength = 600;

    /// <summary>Returns true when the inventory result reports data-quality warnings.</summary>
    internal static bool HasWarnings(string? result) =>
        !string.IsNullOrWhiteSpace(result)
        && (result.Contains(WarningsMarker, StringComparison.OrdinalIgnoreCase)
            || result.Contains(ItemsMarker, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns the data-quality items of the result as one line (for example "1 server has a missing or invalid CPU core count;
    /// 2 servers have missing or invalid memory sizes"), or a shortened result when it lists none.
    /// </summary>
    internal static string Summarize(string result)
    {
        int start = result.IndexOf(ItemsMarker, StringComparison.OrdinalIgnoreCase);
        if (start >= 0)
        {
            var items = new List<string>();
            foreach (string line in result[start..].Split('\n').Skip(1).Select(l => l.Trim()))
            {
                if (line.StartsWith('-') || line.StartsWith('*'))
                {
                    items.Add(line.TrimStart('-', '*', ' ').Replace("**", string.Empty, StringComparison.Ordinal).TrimEnd('.'));
                }
                else if (line.Length > 0 && items.Count > 0)
                {
                    break;
                }
            }

            if (items.Count > 0)
            {
                return Truncate(string.Join("; ", items) + ".");
            }
        }

        return Truncate(result.Replace("**", string.Empty, StringComparison.Ordinal).Trim());
    }

    private static string Truncate(string text) => text.Length <= MaxSummaryLength ? text : text[..MaxSummaryLength] + "…";
}
