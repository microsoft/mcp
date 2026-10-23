// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Azure.Mcp.Tools.Optimization.Models;

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>
/// Parses the alternative resize/SKU options out of an Azure Resource Graph response. The query
/// projects a single <c>alternatives</c> column whose value is a serialized <c>cols</c>/<c>rows</c>
/// table.
/// </summary>
public static class AlternativeRecommendationsArgParser
{
    private const string OptionColumn = "option";
    private const string ObservationWindowDaysColumn = "observationWindowDays";
    private const string RecommendationMessageColumn = "recommendationMessage";
    private const string ProposedSkuColumn = "proposedSku";
    private const string ProposedSeriesColumn = "proposedSeries";
    private const string ProposedProcessorColumn = "proposedProcessor";
    private const string EstimatedMonthlySavingsColumn = "estimatedMonthlySavings";
    private const string SavingsCurrencyColumn = "savingsCurrency";
    private const string EstimatedCoresSavingsColumn = "estimatedCoresSavings";

    /// <summary>
    /// Parses the ARG data rows into alternative recommendations for <paramref name="resourceId"/>.
    /// Malformed rows are skipped.
    /// </summary>
    public static IReadOnlyList<AlternativeRecommendation> Parse(IEnumerable<JsonElement> rows, string resourceId)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var results = new List<AlternativeRecommendation>();
        foreach (var row in rows)
        {
            if (row.ValueKind != JsonValueKind.Object ||
                !row.TryGetProperty("alternatives", out var alternatives))
            {
                continue;
            }

            AppendTable(alternatives, resourceId, results);
        }

        return results;
    }

    private static void AppendTable(
        JsonElement alternatives,
        string resourceId,
        List<AlternativeRecommendation> results)
    {
        // The value may be a materialized object (parse_json succeeded) or, defensively, a JSON
        // string that still needs to be parsed.
        if (alternatives.ValueKind == JsonValueKind.String)
        {
            var raw = alternatives.GetString();
            if (string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            using var nested = JsonDocument.Parse(raw);
            AppendTable(nested.RootElement, resourceId, results);
            return;
        }

        if (alternatives.ValueKind != JsonValueKind.Object ||
            !alternatives.TryGetProperty("cols", out var cols) ||
            cols.ValueKind != JsonValueKind.Array ||
            !alternatives.TryGetProperty("rows", out var rows) ||
            rows.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        var columnIndex = BuildColumnIndex(cols);

        foreach (var rowValues in rows.EnumerateArray())
        {
            if (rowValues.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            var cells = rowValues.EnumerateArray().ToArray();

            results.Add(new AlternativeRecommendation
            {
                ResourceId = resourceId,
                Option = GetInt(cells, columnIndex, OptionColumn) ?? 0,
                ObservationWindowDays = GetInt(cells, columnIndex, ObservationWindowDaysColumn) ?? 0,
                RecommendationMessage = GetString(cells, columnIndex, RecommendationMessageColumn),
                ProposedSku = GetString(cells, columnIndex, ProposedSkuColumn),
                ProposedSeries = GetString(cells, columnIndex, ProposedSeriesColumn),
                ProposedProcessor = GetString(cells, columnIndex, ProposedProcessorColumn),
                EstimatedMonthlySavings = GetDouble(cells, columnIndex, EstimatedMonthlySavingsColumn),
                SavingsCurrency = GetString(cells, columnIndex, SavingsCurrencyColumn),
                EstimatedCoresSavings = GetDouble(cells, columnIndex, EstimatedCoresSavingsColumn),
            });
        }
    }

    private static Dictionary<string, int> BuildColumnIndex(JsonElement cols)
    {
        var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var position = 0;
        foreach (var col in cols.EnumerateArray())
        {
            if (col.ValueKind == JsonValueKind.String)
            {
                var name = col.GetString();
                if (!string.IsNullOrEmpty(name))
                {
                    index[name] = position;
                }
            }

            position++;
        }

        return index;
    }

    private static JsonElement? Cell(JsonElement[] cells, Dictionary<string, int> columnIndex, string column)
    {
        if (!columnIndex.TryGetValue(column, out var position) || position >= cells.Length)
        {
            return null;
        }

        var cell = cells[position];
        return cell.ValueKind == JsonValueKind.Null ? null : cell;
    }

    private static string? GetString(JsonElement[] cells, Dictionary<string, int> columnIndex, string column)
    {
        var cell = Cell(cells, columnIndex, column);
        if (cell is null)
        {
            return null;
        }

        return cell.Value.ValueKind == JsonValueKind.String
            ? cell.Value.GetString()
            : cell.Value.GetRawText();
    }

    private static int? GetInt(JsonElement[] cells, Dictionary<string, int> columnIndex, string column)
    {
        var cell = Cell(cells, columnIndex, column);
        if (cell is null)
        {
            return null;
        }

        return cell.Value.ValueKind switch
        {
            JsonValueKind.Number when cell.Value.TryGetInt32(out var value) => value,
            JsonValueKind.Number when cell.Value.TryGetDouble(out var value) => (int)Math.Round(value),
            JsonValueKind.String when int.TryParse(
                cell.Value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) => value,
            _ => null,
        };
    }

    private static double? GetDouble(JsonElement[] cells, Dictionary<string, int> columnIndex, string column)
    {
        var cell = Cell(cells, columnIndex, column);
        if (cell is null)
        {
            return null;
        }

        return cell.Value.ValueKind switch
        {
            JsonValueKind.Number when cell.Value.TryGetDouble(out var value) => value,
            JsonValueKind.String when double.TryParse(
                cell.Value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) => value,
            _ => null,
        };
    }
}
