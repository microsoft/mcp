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
    /// Renders a chart from the provided Kusto query results.
    /// </summary>
    /// <param name="results">
    /// The raw query result rows. Element [0] is the column-type header dictionary;
    /// subsequent elements are data rows as JSON arrays.
    /// </param>
    /// <param name="chartType">The chart type to render.</param>
    /// <param name="title">Optional chart title.</param>
    /// <returns>A <see cref="ResponseImage"/> containing the rendered PNG.</returns>
    /// <exception cref="ChartRenderingException">
    /// Thrown when the data cannot be rendered as the requested chart type — either because the
    /// shape of the result set does not match (e.g. <see cref="ChartType.TimeSeries"/> requested
    /// without a datetime column) or the underlying rendering pipeline fails. The exception
    /// message is intended to be surfaced verbatim to the caller as a tool-call error.
    /// </exception>
    ResponseImage Render(IReadOnlyList<JsonElement> results, ChartType chartType, string? title = null);
}
