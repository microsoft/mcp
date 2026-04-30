// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Kusto.Rendering;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class ScottPlotChartRendererTests
{
    private static readonly byte[] s_pngMagic = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly ScottPlotChartRenderer _renderer = new(NullLogger<ScottPlotChartRenderer>.Instance);

    // Minimal Kusto result format:
    //   results[0]  = { "ColName": "kqlType", ... }   (schema header)
    //   results[1+] = [ value, value, ... ]             (data rows, in column order)

    private static IReadOnlyList<JsonElement> TimeSeriesResults(int rowCount = 5)
    {
        var rows = new List<string>
        {
            """{"Time":"datetime","Value":"real"}"""
        };
        var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        for (var i = 0; i < rowCount; i++)
        {
            var ts = baseTime.AddMinutes(i).ToString("o");
            var val = 20.0 + i * 2.5;
            rows.Add($"[\"{ts}\",{val}]");
        }
        return rows.Select(r => JsonDocument.Parse(r).RootElement.Clone()).ToList();
    }

    private static IReadOnlyList<JsonElement> BarResults()
    {
        return new[]
        {
            """{"Category":"string","Count":"long"}""",
            """["Alpha",42]""",
            """["Beta",17]""",
            """["Gamma",91]""",
        }.Select(r => JsonDocument.Parse(r).RootElement.Clone()).ToList();
    }

    private static IReadOnlyList<JsonElement> ScatterResults()
    {
        return new[]
        {
            """{"X":"real","Y":"real"}""",
            """[1.0,2.0]""",
            """[3.0,5.0]""",
            """[7.0,3.5]""",
        }.Select(r => JsonDocument.Parse(r).RootElement.Clone()).ToList();
    }

    private static IReadOnlyList<JsonElement> PieResults()
    {
        return new[]
        {
            """{"Slice":"string","Value":"real"}""",
            """["Alpha",30.0]""",
            """["Beta",45.0]""",
            """["Gamma",25.0]""",
        }.Select(r => JsonDocument.Parse(r).RootElement.Clone()).ToList();
    }

    [Fact]
    public void Render_TimeSeries_ReturnsValidPng()
    {
        var result = _renderer.Render(TimeSeriesResults(), ChartType.TimeSeries, "Test TimeSeries");

        Assert.NotNull(result);
        Assert.Equal("image/png", result.MimeType);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length > 1000, $"PNG should be >1KB but was {result.Data.Length} bytes");
        Assert_StartsWith(s_pngMagic, result.Data);
    }

    [Fact]
    public void Render_Bar_ReturnsValidPng()
    {
        var result = _renderer.Render(BarResults(), ChartType.Bar, "Test Bar");

        Assert.NotNull(result);
        Assert.Equal("image/png", result.MimeType);
        Assert.True(result.Data.Length > 1000, $"PNG should be >1KB but was {result.Data.Length} bytes");
        Assert_StartsWith(s_pngMagic, result.Data);
    }

    [Fact]
    public void Render_Scatter_ReturnsValidPng()
    {
        var result = _renderer.Render(ScatterResults(), ChartType.Scatter, "Test Scatter");

        Assert.NotNull(result);
        Assert.Equal("image/png", result.MimeType);
        Assert.True(result.Data.Length > 1000, $"PNG should be >1KB but was {result.Data.Length} bytes");
        Assert_StartsWith(s_pngMagic, result.Data);
    }

    [Fact]
    public void Render_Pie_ReturnsValidPng()
    {
        var result = _renderer.Render(PieResults(), ChartType.Pie, "Test Pie");

        Assert.NotNull(result);
        Assert.Equal("image/png", result.MimeType);
        Assert.True(result.Data.Length > 1000, $"PNG should be >1KB but was {result.Data.Length} bytes");
        Assert_StartsWith(s_pngMagic, result.Data);
    }

    [Fact]
    public void Render_NoRows_ThrowsChartRenderingException()
    {
        var onlyHeader = new[]
        {
            """{"Time":"datetime","Value":"real"}"""
        }.Select(r => JsonDocument.Parse(r).RootElement.Clone()).ToList();

        Assert.Throws<ChartRenderingException>(() =>
            _renderer.Render(onlyHeader, ChartType.TimeSeries));
    }

    [Fact]
    public void Render_EmptyResults_ThrowsChartRenderingException()
    {
        Assert.Throws<ChartRenderingException>(() =>
            _renderer.Render([], ChartType.TimeSeries));
    }

    [Fact]
    public void Render_TimeSeries_WithManyRows_ReturnsValidPng()
    {
        // Simulate a realistic 60-minute timeseries
        var result = _renderer.Render(TimeSeriesResults(60), ChartType.TimeSeries, "60-min CPU");

        Assert.NotNull(result);
        Assert.True(result.Data.Length > 1000);
        Assert_StartsWith(s_pngMagic, result.Data);
    }

    private static void Assert_StartsWith(byte[] expected, byte[] actual)
    {
        Assert.True(actual.Length >= expected.Length,
            $"Data too short: {actual.Length} bytes, expected at least {expected.Length}");
        Assert.Equal(expected, actual[..expected.Length]);
    }
}

