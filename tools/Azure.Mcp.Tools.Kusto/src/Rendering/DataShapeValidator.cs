// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Rendering;

/// <summary>
/// Validates whether the columns in a Kusto result set are compatible with a requested <see cref="ChartType"/>.
/// </summary>
internal static class DataShapeValidator
{
    private static readonly HashSet<string> s_numericTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "long", "real", "double", "decimal"
    };

    private static readonly HashSet<string> s_stringTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "string", "guid", "bool"
    };

    /// <summary>
    /// Validates that the provided column map is compatible with the requested <paramref name="chartType"/>.
    /// </summary>
    /// <param name="columns">Map of column name → KQL type string from the query result header row.</param>
    /// <param name="chartType">The requested chart type to validate against.</param>
    /// <param name="errorMessage">
    /// When validation fails, contains a human-readable description of why the data does not fit
    /// the requested chart type. <see langword="null"/> on success.
    /// </param>
    /// <returns><see langword="true"/> if the columns are compatible; otherwise <see langword="false"/>.</returns>
    public static bool Validate(
        IReadOnlyDictionary<string, string> columns,
        ChartType chartType,
        out string? errorMessage)
    {
        var numericCols = columns.Where(c => s_numericTypes.Contains(c.Value)).Select(c => c.Key).ToList();
        var stringCols = columns.Where(c => s_stringTypes.Contains(c.Value)).Select(c => c.Key).ToList();
        var datetimeCols = columns.Where(c => c.Value.Equals("datetime", StringComparison.OrdinalIgnoreCase)).Select(c => c.Key).ToList();

        switch (chartType)
        {
            case ChartType.TimeSeries:
                if (datetimeCols.Count == 0)
                {
                    errorMessage = $"ChartType.TimeSeries requires at least one 'datetime' column. Found columns: {FormatColumns(columns)}.";
                    return false;
                }
                if (numericCols.Count == 0)
                {
                    errorMessage = $"ChartType.TimeSeries requires at least one numeric column (int/long/real/double/decimal). Found columns: {FormatColumns(columns)}.";
                    return false;
                }
                break;

            case ChartType.Bar:
                if (stringCols.Count == 0)
                {
                    errorMessage = $"ChartType.Bar requires at least one string/label column. Found columns: {FormatColumns(columns)}.";
                    return false;
                }
                if (numericCols.Count == 0)
                {
                    errorMessage = $"ChartType.Bar requires at least one numeric column. Found columns: {FormatColumns(columns)}.";
                    return false;
                }
                break;

            case ChartType.Scatter:
                if (numericCols.Count < 2)
                {
                    errorMessage = $"ChartType.Scatter requires at least two numeric columns (X and Y). Found {numericCols.Count} numeric column(s) in: {FormatColumns(columns)}.";
                    return false;
                }
                break;

            case ChartType.Pie:
                if (stringCols.Count == 0)
                {
                    errorMessage = $"ChartType.Pie requires at least one string/label column for slice labels. Found columns: {FormatColumns(columns)}.";
                    return false;
                }
                if (numericCols.Count == 0)
                {
                    errorMessage = $"ChartType.Pie requires at least one numeric column for slice values. Found columns: {FormatColumns(columns)}.";
                    return false;
                }
                break;
        }

        errorMessage = null;
        return true;
    }

    private static string FormatColumns(IReadOnlyDictionary<string, string> columns)
        => string.Join(", ", columns.Select(c => $"{c.Key}:{c.Value}"));
}
