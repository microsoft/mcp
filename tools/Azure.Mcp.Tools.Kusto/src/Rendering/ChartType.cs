// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Rendering;

/// <summary>
/// Specifies the type of chart to render from Kusto query results.
/// </summary>
public enum ChartType
{
    /// <summary>
    /// A time series line chart. Requires at least one <c>datetime</c> column and one numeric column.
    /// The datetime column is used as the X axis; all numeric columns are plotted as separate lines.
    /// </summary>
    TimeSeries,

    /// <summary>
    /// A bar chart. Requires exactly one string/label column and at least one numeric column.
    /// </summary>
    Bar,

    /// <summary>
    /// A scatter plot. Requires exactly two numeric columns (X and Y).
    /// </summary>
    Scatter,

    /// <summary>
    /// A pie chart. Requires exactly one string/label column and one numeric column for values.
    /// </summary>
    Pie,
}
