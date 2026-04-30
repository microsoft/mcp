// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Rendering;

/// <summary>
/// Renders a chart image from Kusto query result data.
/// </summary>
public interface IKustoChartRenderer
{
    /// <summary>
    /// Attempts to render a chart from the provided Kusto query results.
    /// </summary>
    /// <param name="results">
    /// The raw query result rows. Element [0] is the column-type header dictionary;
    /// subsequent elements are data rows as JSON arrays.
    /// </param>
    /// <param name="chartType">The chart type to render.</param>
    /// <param name="title">Optional chart title.</param>
    /// <returns>
    /// A <see cref="ResponseImage"/> containing the rendered PNG, or <see langword="null"/> if
    /// rendering is not possible (e.g. data shape does not match the chart type, or the result is empty).
    /// </returns>
    ResponseImage? TryRender(IReadOnlyList<JsonElement> results, ChartType chartType, string? title = null);
}
