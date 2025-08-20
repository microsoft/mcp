// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Xml;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using MetricDefinition = Azure.Mcp.Tools.Monitor.Models.MetricDefinition;
using MetricNamespace = Azure.Mcp.Tools.Monitor.Models.MetricNamespace;
using MetricResult = Azure.Mcp.Tools.Monitor.Models.MetricResult;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorMetricsService(IResourceResolverService resourceResolverService, IMetricsQueryClientService metricsQueryClientService)
    : BaseAzureService(), IMonitorMetricsService
{
    private readonly IResourceResolverService _resourceResolverService = resourceResolverService ?? throw new ArgumentNullException(nameof(resourceResolverService));
    private readonly IMetricsQueryClientService _metricsQueryClientService = metricsQueryClientService ?? throw new ArgumentNullException(nameof(metricsQueryClientService));

    public async Task<List<MetricResult>> QueryMetricsAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string metricNamespace,
        IEnumerable<string> metricNames,
        string? startTime = null,
        string? endTime = null,
        string? interval = null,
        string? aggregation = null,
        string? filter = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceName, metricNamespace);
        ArgumentNullException.ThrowIfNull(metricNames);

        var resourceId = await _resourceResolverService.ResolveResourceIdAsync(subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy);
        var client = await _metricsQueryClientService.CreateClientAsync(tenant, retryPolicy);

        // Parse time range
        DateTimeOffset? startTimeOffset = null;
        DateTimeOffset? endTimeOffset = null;

        if (!string.IsNullOrEmpty(startTime))
        {
            if (!DateTimeOffset.TryParse(startTime, out var start))
            {
                throw new ArgumentException($"Invalid start time format: {startTime}");
            }
            startTimeOffset = start;
        }

        if (!string.IsNullOrEmpty(endTime))
        {
            if (!DateTimeOffset.TryParse(endTime, out var end))
            {
                throw new ArgumentException($"Invalid end time format: {endTime}");
            }
            endTimeOffset = end;
        }

        // Build query options
        var queryOptions = new MetricsQueryOptions();

        if (startTimeOffset.HasValue && endTimeOffset.HasValue)
        {
            queryOptions.TimeRange = new QueryTimeRange(startTimeOffset.Value, endTimeOffset.Value);
        }
        else if (startTimeOffset.HasValue)
        {
            queryOptions.TimeRange = new QueryTimeRange(startTimeOffset.Value, DateTimeOffset.UtcNow);
        }
        else if (endTimeOffset.HasValue)
        {
            queryOptions.TimeRange = new QueryTimeRange(endTimeOffset.Value - TimeSpan.FromDays(1), endTimeOffset.Value);
        }
        else
        {
            // Default to last 24 hours if no time range specified
            queryOptions.TimeRange = new QueryTimeRange(TimeSpan.FromDays(1));
        }

        if (!string.IsNullOrEmpty(interval))
        {
            try
            {
                queryOptions.Granularity = XmlConvert.ToTimeSpan(interval);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid interval format: {ex}.", ex);
            }
        }

        if (!string.IsNullOrEmpty(aggregation))
        {
            var aggregationTypes = aggregation.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(a => a.Trim())
                .ToList();
            queryOptions.Aggregations.Clear();
            foreach (var agg in aggregationTypes)
            {
                if (Enum.TryParse<MetricAggregationType>(agg, true, out var aggType))
                {
                    queryOptions.Aggregations.Add(aggType);
                }
            }
        }

        if (!string.IsNullOrEmpty(filter))
        {
            queryOptions.Filter = filter;
        }

        queryOptions.MetricNamespace = metricNamespace;

        // Query metrics
        var response = await client.QueryResourceAsync(
            resourceId,
            metricNames,
            queryOptions);

        // Convert response directly to compact format
        var results = new List<MetricResult>();
        foreach (var metric in response.Value.Metrics)
        {
            var compactResult = new MetricResult
            {
                Name = metric.Name ?? string.Empty,
                Unit = metric.Unit.ToString(),
                TimeSeries = new List<MetricTimeSeries>()
            };

            foreach (var timeSeries in metric.TimeSeries)
            {
                if (timeSeries.Values.Count == 0)
                    continue;

                var compactTimeSeries = new MetricTimeSeries
                {
                    Metadata = new Dictionary<string, string>(),
                    Start = timeSeries.Values.First().TimeStamp.UtcDateTime,
                    End = timeSeries.Values.Last().TimeStamp.UtcDateTime,
                    Interval = interval ?? "PT1M"
                };

                // Add metadata/dimensions
                if (timeSeries.Metadata != null)
                {
                    foreach (var kvp in timeSeries.Metadata)
                    {
                        compactTimeSeries.Metadata[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                    }
                }

                // Extract values into arrays, only including non-null arrays
                var avgValues = timeSeries.Values
                    .Where(v => v.Average.HasValue)
                    .Select(v => v.Average!.Value)
                    .ToArray();
                if (avgValues.Length > 0)
                    compactTimeSeries.AvgBuckets = avgValues;

                var minValues = timeSeries.Values
                    .Where(v => v.Minimum.HasValue)
                    .Select(v => v.Minimum!.Value)
                    .ToArray();
                if (minValues.Length > 0)
                    compactTimeSeries.MinBuckets = minValues;

                var maxValues = timeSeries.Values
                    .Where(v => v.Maximum.HasValue)
                    .Select(v => v.Maximum!.Value)
                    .ToArray();
                if (maxValues.Length > 0)
                    compactTimeSeries.MaxBuckets = maxValues;

                var totalValues = timeSeries.Values
                    .Where(v => v.Total.HasValue)
                    .Select(v => v.Total!.Value)
                    .ToArray();
                if (totalValues.Length > 0)
                    compactTimeSeries.TotalBuckets = totalValues;

                var countValues = timeSeries.Values
                    .Where(v => v.Count.HasValue)
                    .Select(v => v.Count!.Value)
                    .ToArray();
                if (countValues.Length > 0)
                    compactTimeSeries.CountBuckets = countValues;

                compactResult.TimeSeries.Add(compactTimeSeries);
            }

            results.Add(compactResult);
        }

        return results;
    }

    public async Task<List<MetricDefinition>> ListMetricDefinitionsAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string? metricNamespace = null,
        string? searchString = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceName);

        var resourceId = await _resourceResolverService.ResolveResourceIdAsync(subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy);
        var client = await _metricsQueryClientService.CreateClientAsync(tenant, retryPolicy);

        // List metric definitions using the metrics query client
        var response = client.GetMetricDefinitionsAsync(resourceId, metricNamespace);

        var results = new List<MetricDefinition>();
        var pages = response.AsPages();
        await foreach (var page in pages)
        {
            foreach (global::Azure.Monitor.Query.Models.MetricDefinition definition in page.Values)
            {
                if (string.IsNullOrEmpty(definition.Name))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(searchString) &&
                    !definition.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) &&
                    !(definition.DisplayDescription?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    continue;
                }
                var metricDef = new MetricDefinition
                {
                    Dimensions = definition.Dimensions?.ToList() ?? new(),
                    Name = definition.Name,
                    MetricNamespace = definition.Namespace,
                    Description = definition.DisplayDescription,
                    Category = definition.Category,
                    Unit = definition.Unit?.ToString() ?? string.Empty,
                    IsDimensionRequired = definition.IsDimensionRequired,
                    PrimaryAggregationType = definition.PrimaryAggregationType?.ToString()
                };

                // Add supported aggregation types
                if (definition.SupportedAggregationTypes != null)
                {
                    metricDef.SupportedAggregationTypes = [.. definition.SupportedAggregationTypes.Select(a => a.ToString())];
                }

                // Convert metric availabilities to allowed intervals (ISO 8601 duration format)
                if (definition.MetricAvailabilities != null)
                {
                    metricDef.AllowedIntervals = [.. definition.MetricAvailabilities
                        .Where(a => a.Granularity.HasValue)
                        .Select(a => XmlConvert.ToString(a.Granularity!.Value))
                        .Distinct()
                        .OrderBy(interval => interval)];
                }

                results.Add(metricDef);
            }
        }
        return results;
    }

    public async Task<List<MetricNamespace>> ListMetricNamespacesAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string? searchString = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceName);

        var resourceId = await _resourceResolverService.ResolveResourceIdAsync(subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy);
        var client = await _metricsQueryClientService.CreateClientAsync(tenant, retryPolicy);

        // List metric namespaces using the metrics query client
        var response = client.GetMetricNamespacesAsync(resourceId);

        var results = new List<MetricNamespace>();
        var pages = response.AsPages();
        await foreach (var page in pages)
        {
            foreach (global::Azure.Monitor.Query.Models.MetricNamespace ns in page.Values)
            {
                // Apply search string filtering if provided
                if (!string.IsNullOrEmpty(searchString) &&
                    !(ns.Name?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    continue;
                }

                results.Add(new MetricNamespace
                {
                    Name = ns.FullyQualifiedName,
                    Type = ns.Type ?? string.Empty,
                    ClassificationType = ns.Classification?.ToString() ?? string.Empty
                });
            }
        }

        return results;
    }
}
