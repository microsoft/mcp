// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Rendering;

/// <summary>
/// Renders Kusto query result data as chart images using ScottPlot 5.x (MIT license).
/// </summary>
public sealed class ScottPlotChartRenderer(ILogger<ScottPlotChartRenderer> logger) : IKustoChartRenderer
{
    private const int ChartWidth = 960;
    private const int ChartHeight = 480;
    private const string ImageMimeType = "image/png";

    private static readonly HashSet<string> s_numericKqlTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "long", "real", "double", "decimal"
    };

    /// <inheritdoc />
    public ResponseImage Render(IReadOnlyList<JsonElement> results, ChartType chartType, string? title = null)
    {
        if (results.Count < 2)
        {
            throw new ChartRenderingException(
                $"Cannot render {chartType} chart: query returned no rows (only the schema header). Adjust the query so it returns at least one data row.");
        }

        // results[0] is the column→type header dictionary
        if (results[0].ValueKind != JsonValueKind.Object)
        {
            throw new ChartRenderingException(
                $"Cannot render {chartType} chart: query result is missing the expected column-type header.");
        }

        var columns = results[0]
            .EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString() ?? string.Empty);

        if (!DataShapeValidator.Validate(columns, chartType, out var errorMessage))
        {
            throw new ChartRenderingException(errorMessage!);
        }

        // Data rows are JSON arrays (one element per column in column order)
        var columnNames = columns.Keys.ToList();
        var rows = results.Skip(1)
            .Where(r => r.ValueKind == JsonValueKind.Array)
            .ToList();

        if (rows.Count == 0)
        {
            throw new ChartRenderingException(
                $"Cannot render {chartType} chart: query returned no data rows.");
        }

        try
        {
            return chartType switch
            {
                ChartType.TimeSeries => RenderTimeSeries(columnNames, columns, rows, title),
                ChartType.Bar => RenderBar(columnNames, columns, rows, title),
                ChartType.Scatter => RenderScatter(columnNames, columns, rows, title),
                ChartType.Pie => RenderPie(columnNames, columns, rows, title),
                _ => throw new ChartRenderingException($"Unsupported chart type: {chartType}.")
            };
        }
        catch (ChartRenderingException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Chart rendering failed for {ChartType}", chartType);
            throw new ChartRenderingException(
                $"Failed to render {chartType} chart: {ex.Message}", ex);
        }
    }

    private ResponseImage RenderTimeSeries(
        IList<string> columnNames,
        IReadOnlyDictionary<string, string> columns,
        IList<JsonElement> rows,
        string? title)
    {
        var datetimeIdx = columnNames
            .Select((name, i) => (name, i))
            .First(t => columns[t.name].Equals("datetime", StringComparison.OrdinalIgnoreCase))
            .i;

        var numericIndices = columnNames
            .Select((name, i) => (name, i))
            .Where(t => s_numericKqlTypes.Contains(columns[t.name]))
            .ToList();

        var timestamps = new List<double>();
        var seriesData = numericIndices.ToDictionary(t => t.name, _ => new List<double>());

        foreach (var row in rows)
        {
            var arr = row.EnumerateArray().ToList();
            if (arr.Count <= datetimeIdx) continue;

            var dtRaw = arr[datetimeIdx];
            if (!TryParseDateTime(dtRaw, out var dt)) continue;
            timestamps.Add(dt.ToOADate());

            foreach (var (colName, colIdx) in numericIndices)
            {
                seriesData[colName].Add(colIdx < arr.Count ? ToDouble(arr[colIdx]) : 0.0);
            }
        }

        if (timestamps.Count == 0)
        {
            throw new ChartRenderingException(
                "TimeSeries chart could not be rendered: none of the rows contained a parseable datetime value.");
        }

        var xs = timestamps.ToArray();
        var plot = new ScottPlot.Plot();
        plot.Title(title ?? "Time Series");
        plot.Axes.DateTimeTicksBottom();
        plot.ShowLegend();

        foreach (var (colName, values) in seriesData)
        {
            var ys = values.ToArray();
            var scatter = plot.Add.Scatter(xs, ys);
            scatter.LegendText = colName;
        }

        plot.Axes.AutoScale();
        return ToPngImage(plot, $"Time series chart with {timestamps.Count} data points");
    }

    private ResponseImage RenderBar(
        IList<string> columnNames,
        IReadOnlyDictionary<string, string> columns,
        IList<JsonElement> rows,
        string? title)
    {
        var labelIdx = columnNames
            .Select((name, i) => (name, i))
            .First(t => !s_numericKqlTypes.Contains(columns[t.name]) &&
                        !columns[t.name].Equals("datetime", StringComparison.OrdinalIgnoreCase))
            .i;

        var valueIdx = columnNames
            .Select((name, i) => (name, i))
            .First(t => s_numericKqlTypes.Contains(columns[t.name]))
            .i;

        var labels = new List<string>();
        var values = new List<double>();

        foreach (var row in rows)
        {
            var arr = row.EnumerateArray().ToList();
            if (arr.Count <= Math.Max(labelIdx, valueIdx)) continue;
            labels.Add(arr[labelIdx].ValueKind == JsonValueKind.Null ? "" : arr[labelIdx].ToString());
            values.Add(ToDouble(arr[valueIdx]));
        }

        if (values.Count == 0)
        {
            throw new ChartRenderingException(
                "Bar chart could not be rendered: rows did not contain valid label/value pairs.");
        }

        var plot = new ScottPlot.Plot();
        plot.Title(title ?? columnNames[valueIdx]);
        plot.YLabel(columnNames[valueIdx]);

        var bars = values.Select((v, i) =>
            new ScottPlot.Bar { Position = i, Value = v, Label = i < labels.Count ? labels[i] : "" }).ToArray();
        var barPlot = plot.Add.Bars(bars);
        barPlot.ValueLabelStyle.Bold = true;

        var tickPositions = Enumerable.Range(0, values.Count).Select(i => (double)i).ToArray();
        var tickLabels = values.Select((_, i) => i < labels.Count ? labels[i] : i.ToString()).ToArray();
        plot.Axes.Bottom.SetTicks(tickPositions, tickLabels);
        plot.Axes.Bottom.TickLabelStyle.Rotation = labels.Count > 6 ? 45 : 0;

        plot.Axes.AutoScale();
        return ToPngImage(plot, $"Bar chart with {values.Count} categories");
    }

    private ResponseImage RenderScatter(
        IList<string> columnNames,
        IReadOnlyDictionary<string, string> columns,
        IList<JsonElement> rows,
        string? title)
    {
        var numericIndices = columnNames
            .Select((name, i) => (name, i))
            .Where(t => s_numericKqlTypes.Contains(columns[t.name]))
            .Take(2)
            .ToList();

        var xIdx = numericIndices[0].i;
        var yIdx = numericIndices[1].i;

        var xs = new List<double>();
        var ys = new List<double>();

        foreach (var row in rows)
        {
            var arr = row.EnumerateArray().ToList();
            if (arr.Count <= Math.Max(xIdx, yIdx)) continue;
            xs.Add(ToDouble(arr[xIdx]));
            ys.Add(ToDouble(arr[yIdx]));
        }

        if (xs.Count == 0)
        {
            throw new ChartRenderingException(
                "Scatter chart could not be rendered: rows did not contain valid X/Y numeric pairs.");
        }

        var plot = new ScottPlot.Plot();
        plot.Title(title ?? $"{columnNames[xIdx]} vs {columnNames[yIdx]}");
        plot.XLabel(columnNames[xIdx]);
        plot.YLabel(columnNames[yIdx]);
        plot.Add.Scatter(xs.ToArray(), ys.ToArray());
        plot.Axes.AutoScale();
        return ToPngImage(plot, $"Scatter plot with {xs.Count} data points");
    }

    private ResponseImage RenderPie(
        IList<string> columnNames,
        IReadOnlyDictionary<string, string> columns,
        IList<JsonElement> rows,
        string? title)
    {
        var labelIdx = columnNames
            .Select((name, i) => (name, i))
            .First(t => !s_numericKqlTypes.Contains(columns[t.name]) &&
                        !columns[t.name].Equals("datetime", StringComparison.OrdinalIgnoreCase))
            .i;

        var valueIdx = columnNames
            .Select((name, i) => (name, i))
            .First(t => s_numericKqlTypes.Contains(columns[t.name]))
            .i;

        var slices = new List<ScottPlot.PieSlice>();
        foreach (var row in rows)
        {
            var arr = row.EnumerateArray().ToList();
            if (arr.Count <= Math.Max(labelIdx, valueIdx)) continue;
            var val = ToDouble(arr[valueIdx]);
            if (val <= 0) continue;
            slices.Add(new ScottPlot.PieSlice
            {
                Value = val,
                Label = arr[labelIdx].ValueKind == JsonValueKind.Null ? "" : arr[labelIdx].ToString()
            });
        }

        if (slices.Count == 0)
        {
            throw new ChartRenderingException(
                "Pie chart could not be rendered: no rows contained a positive numeric value to chart.");
        }

        var plot = new ScottPlot.Plot();
        plot.Title(title ?? columnNames[valueIdx]);
        var pie = plot.Add.Pie(slices);
        pie.ExplodeFraction = 0.05;
        pie.SliceLabelDistance = 1.35;
        plot.ShowLegend();
        plot.Axes.Frameless();
        plot.HideGrid();

        return ToPngImage(plot, $"Pie chart with {slices.Count} slices");
    }

    private static ResponseImage ToPngImage(ScottPlot.Plot plot, string altText)
    {
        var pngBytes = plot.GetImageBytes(ChartWidth, ChartHeight);
        return new ResponseImage(pngBytes, ImageMimeType, altText);
    }

    private static bool TryParseDateTime(JsonElement element, out DateTime result)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var s = element.GetString();
            if (s != null && DateTime.TryParse(s, null, System.Globalization.DateTimeStyles.RoundtripKind, out result))
                return true;
        }
        result = default;
        return false;
    }

    private static double ToDouble(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Number => element.TryGetDouble(out var d) ? d : 0.0,
        JsonValueKind.String => double.TryParse(element.GetString(), out var d) ? d : 0.0,
        JsonValueKind.True => 1.0,
        JsonValueKind.False => 0.0,
        _ => 0.0
    };
}
