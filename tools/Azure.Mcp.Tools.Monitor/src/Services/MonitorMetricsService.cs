// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Xml;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Monitor;
using Azure.ResourceManager.Monitor.Models;
using MetricDefinition = Azure.Mcp.Tools.Monitor.Models.MetricDefinition;
using MetricNamespace = Azure.Mcp.Tools.Monitor.Models.MetricNamespace;
using MetricResult = Azure.Mcp.Tools.Monitor.Models.MetricResult;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorMetricsService(IResourceResolverService resourceResolverService, ITenantService tenantService)
    : BaseAzureService(tenantService), IMonitorMetricsService
{
    private readonly IResourceResolverService _resourceResolverService = resourceResolverService ?? throw new ArgumentNullException(nameof(resourceResolverService));

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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceName), resourceName), (nameof(metricNamespace), metricNamespace));
        ArgumentNullException.ThrowIfNull(metricNames);

        var resourceId = await _resourceResolverService.ResolveResourceIdAsync(subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy, cancellationToken);
        if (string.IsNullOrEmpty(resourceId))
        {
            throw new ArgumentException($"Resource '{resourceName}' not found or could not be resolved.");
        }

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

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

        // Build query options for new API
        var options = new ArmResourceGetMonitorMetricsOptions
        {
            Metricnames = string.Join(",", metricNames),
            Metricnamespace = metricNamespace
        };

        // Set timespan with proper ISO 8601 format
        if (startTimeOffset.HasValue && endTimeOffset.HasValue)
        {
            options.Timespan = $"{ToIsoString(startTimeOffset.Value)}/{ToIsoString(endTimeOffset.Value)}";
        }
        else if (startTimeOffset.HasValue)
        {
            options.Timespan = $"{ToIsoString(startTimeOffset.Value)}/{ToIsoString(DateTimeOffset.UtcNow)}";
        }
        else if (endTimeOffset.HasValue)
        {
            var defaultStart = endTimeOffset.Value - TimeSpan.FromDays(1);
            options.Timespan = $"{ToIsoString(defaultStart)}/{ToIsoString(endTimeOffset.Value)}";
        }
        else
        {
            // Default to last 24 hours if no time range specified
            var defaultEnd = DateTimeOffset.UtcNow;
            var defaultStart = defaultEnd - TimeSpan.FromDays(1);

            options.Timespan = $"{ToIsoString(defaultStart)}/{ToIsoString(defaultEnd)}";
        }

        if (!string.IsNullOrEmpty(interval))
        {
            try
            {
                var granularity = XmlConvert.ToTimeSpan(interval);
                options.Interval = granularity;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid interval format: {ex}.", ex);
            }
        }

        if (!string.IsNullOrEmpty(aggregation))
        {
            options.Aggregation = aggregation;
        }

        if (!string.IsNullOrEmpty(filter))
        {
            options.Filter = filter;
        }

        // Query metrics using new API
        var metricsPageable = armClient.GetMonitorMetricsAsync(
            new ResourceIdentifier(resourceId!),
            options,
            cancellationToken);

        // Convert response directly to compact format
        var results = new List<MetricResult>();
        await foreach (var metric in metricsPageable.WithCancellation(cancellationToken))
        {
            var compactResult = new MetricResult
            {
                Name = metric.Name?.Value ?? string.Empty,
                Unit = metric.Unit.ToString(),
                TimeSeries = new List<MetricTimeSeries>()
            };

            foreach (var timeSeries in metric.Timeseries)
            {
                if (timeSeries.Data.Count == 0)
                    continue;

                var compactTimeSeries = new MetricTimeSeries
                {
                    Metadata = new Dictionary<string, string>(),
                    Start = timeSeries.Data.First().TimeStamp.UtcDateTime,
                    End = timeSeries.Data.Last().TimeStamp.UtcDateTime,
                    Interval = interval ?? "PT1M"
                };

                // Add metadata/dimensions
                if (timeSeries.Metadatavalues != null)
                {
                    foreach (var metadata in timeSeries.Metadatavalues)
                    {
                        if (metadata.Name?.Value != null && metadata.Value != null)
                        {
                            compactTimeSeries.Metadata[metadata.Name.Value] = metadata.Value;
                        }
                    }
                }

                // Extract values into arrays, only including non-null arrays
                var avgValues = timeSeries.Data
                    .Where(v => v.Average.HasValue)
                    .Select(v => v.Average!.Value)
                    .ToArray();
                if (avgValues.Length > 0)
                    compactTimeSeries.AvgBuckets = avgValues;

                var minValues = timeSeries.Data
                    .Where(v => v.Minimum.HasValue)
                    .Select(v => v.Minimum!.Value)
                    .ToArray();
                if (minValues.Length > 0)
                    compactTimeSeries.MinBuckets = minValues;

                var maxValues = timeSeries.Data
                    .Where(v => v.Maximum.HasValue)
                    .Select(v => v.Maximum!.Value)
                    .ToArray();
                if (maxValues.Length > 0)
                    compactTimeSeries.MaxBuckets = maxValues;

                var totalValues = timeSeries.Data
                    .Where(v => v.Total.HasValue)
                    .Select(v => v.Total!.Value)
                    .ToArray();
                if (totalValues.Length > 0)
                    compactTimeSeries.TotalBuckets = totalValues;

                var countValues = timeSeries.Data
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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceName), resourceName));

        var resourceId = await _resourceResolverService.ResolveResourceIdAsync(subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy, cancellationToken);
        if (string.IsNullOrEmpty(resourceId))
        {
            throw new ArgumentException($"Resource '{resourceName}' not found or could not be resolved.");
        }
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // List metric definitions using the new API
        var definitionsPageable = armClient.GetMonitorMetricDefinitionsAsync(
            new ResourceIdentifier(resourceId!),
            metricNamespace,
            cancellationToken);

        var results = new List<MetricDefinition>();
        await foreach (var definition in definitionsPageable.WithCancellation(cancellationToken))
        {
            var definitionName = definition.Name?.Value;
            if (string.IsNullOrEmpty(definitionName))
            {
                continue;
            }
            if (!string.IsNullOrEmpty(searchString) &&
                !definitionName.Contains(searchString, StringComparison.OrdinalIgnoreCase) &&
                !(definition.DisplayDescription?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                continue;
            }
            var metricDef = new MetricDefinition
            {
                Dimensions = definition.Dimensions?.Select(d => d.Value).ToList() ?? new(),
                Name = definitionName,
                MetricNamespace = definition.Namespace,
                Description = definition.DisplayDescription,
                Category = definition.Category,
                Unit = definition.Unit?.ToString() ?? string.Empty,
                IsDimensionRequired = definition.IsDimensionRequired ?? false,
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
                    .Where(a => a.TimeGrain.HasValue)
                    .Select(a => XmlConvert.ToString(a.TimeGrain!.Value))
                    .Distinct()
                    .OrderBy(interval => interval)];
            }

            results.Add(metricDef);
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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription), (nameof(resourceName), resourceName));

        var resourceId = await _resourceResolverService.ResolveResourceIdAsync(subscription, resourceGroup, resourceType, resourceName, tenant, retryPolicy, cancellationToken);
        if (string.IsNullOrEmpty(resourceId))
        {
            throw new ArgumentException($"Resource '{resourceName}' not found or could not be resolved.");
        }
        var armClient = await CreateArmClientAsync(tenant, retryPolicy, cancellationToken: cancellationToken);

        // List metric namespaces using the new API
        var namespacesPageable = armClient.GetMonitorMetricNamespacesAsync(
            new ResourceIdentifier(resourceId!),
            cancellationToken: cancellationToken);

        var results = new List<MetricNamespace>();
        await foreach (var ns in namespacesPageable.WithCancellation(cancellationToken))
        {
            var namespaceName = ns.Name;
            // Apply search string filtering if provided
            if (!string.IsNullOrEmpty(searchString) &&
                !(namespaceName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                continue;
            }

            results.Add(new MetricNamespace
            {
                Name = ns.MetricNamespaceNameValue ?? namespaceName ?? string.Empty,
                Type = ns.ResourceType.ToString(),
                ClassificationType = ns.Classification?.ToString() ?? string.Empty
            });
        }

        return results;
    }

    private string ToIsoString(DateTimeOffset dto)
    {
        if (dto.Offset == TimeSpan.Zero)
        {
            const string utcFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
            return dto.ToString(utcFormat, CultureInfo.InvariantCulture);
        }
        else
        {
            return dto.ToString("O", CultureInfo.InvariantCulture);
        }
    }
}
