// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// cSpell:ignore metricnames todouble todynamic

using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Xml;
using Azure.Core;
using Azure.Mcp.Tools.Optimization.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>
/// Reads CPU, memory, and network utilization for a compute resource from Azure Monitor metrics,
/// with a VM Insights (Log Analytics) fallback for memory when the metric is not exposed.
/// </summary>
internal sealed class OptimizationMonitorClient(
    HttpClient httpClient,
    TokenCredential credential,
    string armHost,
    string armScope,
    ILogger logger)
{
    private const string PercentageCpu = "Percentage CPU";
    private const string AvailableMemoryPercentage = "Available Memory Percentage";
    private const string NetworkInTotal = "Network In Total";
    private const string NetworkOutTotal = "Network Out Total";
    private static readonly TimeSpan NetworkSampleInterval = TimeSpan.FromMinutes(1);
    private const string LogsScope = "https://api.loganalytics.io/.default";

    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly TokenCredential _credential = credential ?? throw new ArgumentNullException(nameof(credential));
    private readonly string _armHost = armHost.TrimEnd('/');
    private readonly string _armScope = armScope;
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<AzureMonitorUtilizationData> GetUtilizationAsync(
        string resourceId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        TimeSpan interval,
        CancellationToken cancellationToken)
    {
        var cpuTask = QueryMetricsAsync(
            resourceId, new[] { PercentageCpu }, "maximum", startTime, endTime, interval, cancellationToken);
        var networkTask = QueryMetricsAsync(
            resourceId, new[] { NetworkInTotal, NetworkOutTotal }, "total", startTime, endTime, NetworkSampleInterval, cancellationToken);
        var memoryMetricTask = QueryOptionalMemoryMetricAsync(
            resourceId, startTime, endTime, interval, cancellationToken);

        await Task.WhenAll(cpuTask, networkTask, memoryMetricTask).ConfigureAwait(false);

        var result = new AzureMonitorUtilizationData();
        Copy(cpuTask.Result, PercentageCpu, result.CpuMaximumPercent);
        Copy(networkTask.Result, NetworkInTotal, result.NetworkInTotalBytes);
        Copy(networkTask.Result, NetworkOutTotal, result.NetworkOutTotalBytes);

        if (memoryMetricTask.Result.TryGetValue(AvailableMemoryPercentage, out var availableMemory)
            && availableMemory.Count > 0)
        {
            foreach (var pair in availableMemory)
            {
                if (pair.Value is > 0 and <= 100)
                {
                    result.UsedMemoryMaximumPercent[pair.Key] = 100.0 - pair.Value;
                }
            }
        }

        if (result.UsedMemoryMaximumPercent.Count == 0)
        {
            var memory = await QueryMemoryLogsAsync(
                resourceId, startTime, endTime, interval, cancellationToken).ConfigureAwait(false);
            foreach (var pair in memory.Values)
            {
                result.UsedMemoryMaximumPercent[pair.Key] = pair.Value;
            }

            result.MemoryUnavailableReason = memory.UnavailableReason;
        }

        return result;
    }

    private async Task<IReadOnlyDictionary<string, Dictionary<DateTimeOffset, double>>> QueryOptionalMemoryMetricAsync(
        string resourceId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        TimeSpan interval,
        CancellationToken cancellationToken)
    {
        try
        {
            return await QueryMetricsAsync(
                resourceId, new[] { AvailableMemoryPercentage }, "minimum", startTime, endTime, interval, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
        {
            _logger.LogInformation(
                "Available-memory metric is not exposed for the resource; trying VM Insights.");
            return new Dictionary<string, Dictionary<DateTimeOffset, double>>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private async Task<IReadOnlyDictionary<string, Dictionary<DateTimeOffset, double>>> QueryMetricsAsync(
        string resourceId,
        IReadOnlyList<string> metricNames,
        string aggregation,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        TimeSpan interval,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, Dictionary<DateTimeOffset, double>>(StringComparer.OrdinalIgnoreCase);
        var maximumChunk = interval <= TimeSpan.FromMinutes(1)
            ? TimeSpan.FromDays(1)
            : endTime - startTime;

        for (var chunkStart = startTime; chunkStart < endTime; chunkStart += maximumChunk)
        {
            var chunkEnd = chunkStart + maximumChunk;
            if (chunkEnd > endTime)
            {
                chunkEnd = endTime;
            }

            var chunk = await QueryMetricsRangeAsync(
                resourceId, metricNames, aggregation, chunkStart, chunkEnd, interval, cancellationToken)
                .ConfigureAwait(false);

            foreach (var metric in chunk)
            {
                if (!result.TryGetValue(metric.Key, out var values))
                {
                    values = new Dictionary<DateTimeOffset, double>();
                    result[metric.Key] = values;
                }

                foreach (var point in metric.Value)
                {
                    values[point.Key] = point.Value;
                }
            }
        }

        return result;
    }

    private async Task<IReadOnlyDictionary<string, Dictionary<DateTimeOffset, double>>> QueryMetricsRangeAsync(
        string resourceId,
        IReadOnlyList<string> metricNames,
        string aggregation,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        TimeSpan interval,
        CancellationToken cancellationToken)
    {
        var timespan = $"{startTime.UtcDateTime:O}/{endTime.UtcDateTime:O}";
        var uri =
            $"{_armHost}{resourceId}/providers/Microsoft.Insights/metrics" +
            $"?api-version=2023-10-01" +
            $"&metricnames={Uri.EscapeDataString(string.Join(",", metricNames))}" +
            $"&timespan={Uri.EscapeDataString(timespan)}" +
            $"&interval={Uri.EscapeDataString(XmlConvert.ToString(interval))}" +
            $"&aggregation={aggregation}";

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await SendAsync(request, _armScope, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var result = new Dictionary<string, Dictionary<DateTimeOffset, double>>(StringComparer.OrdinalIgnoreCase);
        if (!document.RootElement.TryGetProperty("value", out var metrics)
            || metrics.ValueKind != JsonValueKind.Array)
        {
            return result;
        }

        foreach (var metric in metrics.EnumerateArray())
        {
            var metricName = metric.GetProperty("name").GetProperty("value").GetString();
            if (string.IsNullOrWhiteSpace(metricName))
            {
                continue;
            }

            var values = new Dictionary<DateTimeOffset, double>();
            if (metric.TryGetProperty("timeseries", out var timeSeriesCollection))
            {
                foreach (var timeSeries in timeSeriesCollection.EnumerateArray())
                {
                    if (!timeSeries.TryGetProperty("data", out var data))
                    {
                        continue;
                    }

                    foreach (var point in data.EnumerateArray())
                    {
                        if (!point.TryGetProperty("timeStamp", out var timestampElement)
                            || !timestampElement.TryGetDateTimeOffset(out var timestamp)
                            || !point.TryGetProperty(aggregation, out var aggregateElement)
                            || !aggregateElement.TryGetDouble(out var aggregate))
                        {
                            continue;
                        }

                        if (values.TryGetValue(timestamp, out var existing))
                        {
                            values[timestamp] = aggregation == "maximum"
                                ? Math.Max(existing, aggregate)
                                : existing + aggregate;
                        }
                        else
                        {
                            values[timestamp] = aggregate;
                        }
                    }
                }
            }

            result[metricName] = values;
        }

        return result;
    }

    private async Task<MemoryQueryResult> QueryMemoryLogsAsync(
        string resourceId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        TimeSpan interval,
        CancellationToken cancellationToken)
    {
        var query = $"""
            let startTime = datetime({startTime.UtcDateTime:O});
            let endTime = datetime({endTime.UtcDateTime:O});
            InsightsMetrics
            | where TimeGenerated >= startTime and TimeGenerated < endTime
            | where _ResourceId =~ '{EscapeKqlString(resourceId)}'
            | where Origin == "vm.azm.ms"
            | where Namespace == "Memory" and Name == "AvailableMB"
            | extend TotalMB = todouble(todynamic(Tags)["vm.azm.ms/memorySizeMB"])
            | where TotalMB > 0
            | extend UsedMemoryPercent = 100.0 * (1.0 - todouble(Val) / TotalMB)
            | summarize UsedMemoryPercent=max(UsedMemoryPercent) by Timestamp=bin(TimeGenerated, {FormatKustoTimespan(interval)})
            | order by Timestamp asc
            """;

        try
        {
            var endpoint = $"https://api.loganalytics.io/v1{resourceId}/query";
            var body = BuildLogsQueryBody(query, $"{startTime.UtcDateTime:O}/{endTime.UtcDateTime:O}");
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            };
            using var response = await SendAsync(request, LogsScope, cancellationToken).ConfigureAwait(false);

            var values = new Dictionary<DateTimeOffset, double>();
            if (response.StatusCode is HttpStatusCode.BadRequest
                or HttpStatusCode.Forbidden
                or HttpStatusCode.NotFound)
            {
                _logger.LogWarning(
                    "VM Insights memory query returned {StatusCode}.", (int)response.StatusCode);
                return new MemoryQueryResult(
                    values,
                    "VM Insights memory data is unavailable. Ensure AMA, VM Insights, and its data collection rule are configured.");
            }

            await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            ReadMemoryRows(document.RootElement, values);

            return values.Count == 0
                ? new MemoryQueryResult(values, "VM Insights returned no memory data. Ensure AMA and VM Insights are enabled.")
                : new MemoryQueryResult(values, null);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.BadRequest
                                              or HttpStatusCode.Forbidden
                                              or HttpStatusCode.NotFound)
        {
            _logger.LogWarning(ex, "VM Insights memory data is unavailable.");
            return new MemoryQueryResult(
                new Dictionary<DateTimeOffset, double>(),
                "VM Insights memory data is unavailable. Ensure AMA, VM Insights, and its data collection rule are configured.");
        }
    }

    private static void Copy(
        IReadOnlyDictionary<string, Dictionary<DateTimeOffset, double>> source,
        string metricName,
        Dictionary<DateTimeOffset, double> destination)
    {
        if (!source.TryGetValue(metricName, out var values))
        {
            return;
        }

        foreach (var pair in values)
        {
            destination[pair.Key] = pair.Value;
        }
    }

    private static string EscapeKqlString(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static string BuildLogsQueryBody(string query, string timespan)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            writer.WriteString("query", query);
            writer.WriteString("timespan", timespan);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static string FormatKustoTimespan(TimeSpan interval) =>
        interval.TotalMinutes.ToString("0", CultureInfo.InvariantCulture) + "m";

    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        string scope,
        CancellationToken cancellationToken)
    {
        var token = await _credential
            .GetTokenAsync(new TokenRequestContext(new[] { scope }), cancellationToken)
            .ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new HttpRequestException(
            $"Azure Monitor request failed with status {(int)response.StatusCode}: {responseBody}",
            null,
            response.StatusCode);
    }

    private static void ReadMemoryRows(
        JsonElement root,
        Dictionary<DateTimeOffset, double> values)
    {
        if (!root.TryGetProperty("tables", out var tables)
            || tables.ValueKind != JsonValueKind.Array
            || tables.GetArrayLength() == 0)
        {
            return;
        }

        var table = tables[0];
        var columns = table.GetProperty("columns")
            .EnumerateArray()
            .Select((column, index) => (Name: column.GetProperty("name").GetString(), Index: index))
            .ToDictionary(pair => pair.Name ?? string.Empty, pair => pair.Index, StringComparer.OrdinalIgnoreCase);
        if (!columns.TryGetValue("Timestamp", out var timestampIndex)
            || !columns.TryGetValue("UsedMemoryPercent", out var valueIndex))
        {
            return;
        }

        foreach (var row in table.GetProperty("rows").EnumerateArray())
        {
            var cells = row.EnumerateArray().ToArray();
            if (cells.Length <= Math.Max(timestampIndex, valueIndex)
                || !DateTimeOffset.TryParse(
                    cells[timestampIndex].GetString(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var timestamp)
                || !cells[valueIndex].TryGetDouble(out var percentage)
                || !double.IsFinite(percentage))
            {
                continue;
            }

            values[timestamp] = percentage;
        }
    }

    private sealed record MemoryQueryResult(
        Dictionary<DateTimeOffset, double> Values,
        string? UnavailableReason);
}
