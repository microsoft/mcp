// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Options.Metrics;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Metrics;

/// <summary>
/// Command for querying Azure Monitor metrics across multiple resources in a single batch request
/// </summary>
[CommandMetadata(
    Id = "6c1b0f5f-04c1-4b2e-8f0b-0d6f4f7cba2e",
    Name = "batchquery",
    Title = "Query Azure Monitor Metrics for Multiple Resources",
    Description = "Query Azure Monitor metrics for multiple resources in a single batch request. Returns time series data for the specified metrics, grouped by resource. All resources must belong to the same subscription, Azure region, and resource type.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MetricsBatchQueryCommand(ILogger<MetricsBatchQueryCommand> logger, IMonitorMetricsService metricsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MetricsBatchQueryOptions, MetricsBatchQueryCommand.MetricsBatchQueryCommandResult>(subscriptionResolver)
{
    private readonly ILogger<MetricsBatchQueryCommand> _logger = logger;
    private readonly IMonitorMetricsService _metricsService = metricsService;

    public override void ValidateOptions(MetricsBatchQueryOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.Resources))
        {
            validationResult.Errors.Add($"Invalid format for '--resources'. Provide a comma-separated list of resource names or resource IDs to query (e.g. resource1,resource2).");
        }
        else
        {
            string[] resources = [.. options.Resources.Split(',').Select(t => t.Trim())];
            if (resources.Length == 0 || resources.Any(s => string.IsNullOrWhiteSpace(s)))
            {
                validationResult.Errors.Add($"Invalid format for '--resources'. Provide a comma-separated list of resource names or resource IDs to query (e.g. resource1,resource2).");
            }
        }

        if (string.IsNullOrWhiteSpace(options.MetricNames))
        {
            validationResult.Errors.Add($"Invalid format for '--metric-names'. Provide a comma-separated list of metric names to query (e.g. CPU,memory).");
        }
        else
        {
            string[] metricNames = [.. options.MetricNames.Split(',').Select(t => t.Trim())];

            if (metricNames.Length == 0 || metricNames.Any(s => string.IsNullOrWhiteSpace(s)))
            {
                validationResult.Errors.Add($"Invalid format for '--metric-names'. Provide a comma-separated list of metric names to query (e.g. CPU,memory).");
            }
        }
    }

    public override void PostBindOptions(MetricsBatchQueryOptions options)
    {
        base.PostBindOptions(options);
        options.StartTime ??= DateTime.UtcNow.AddHours(-24).ToString("o"); // Default to 24 hours ago if not specified
        options.EndTime ??= DateTime.UtcNow.ToString("o"); // Default to now if not specified
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MetricsBatchQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string[] resources = [.. options.Resources.Split(',').Select(t => t.Trim())];
            string[] metricNames = [.. options.MetricNames.Split(',').Select(t => t.Trim())];

            var results = await _metricsService.QueryMetricsBatchAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.ResourceType,
                resources,
                options.MetricNamespace,
                metricNames,
                options.StartTime,
                options.EndTime,
                options.Interval,
                options.Aggregation,
                options.Filter,
                options.OrderBy,
                options.Top,
                options.Tenant,
                cancellationToken);

            // Validate bucket count limit
            if (results?.Count > 0)
            {
                int maxBuckets = options.MaxBuckets ?? 50; // Use provided value or default to 50

                foreach (var resourceResult in results)
                {
                    foreach (var metric in resourceResult.Metrics)
                    {
                        foreach (var timeSeries in metric.TimeSeries)
                        {
                            // Check each bucket array for exceeding the limit
                            var bucketCounts = new[]
                            {
                                timeSeries.AvgBuckets?.Length ?? 0,
                                timeSeries.MinBuckets?.Length ?? 0,
                                timeSeries.MaxBuckets?.Length ?? 0,
                                timeSeries.TotalBuckets?.Length ?? 0,
                                timeSeries.CountBuckets?.Length ?? 0
                            };

                            int maxBucketCount = bucketCounts.Max();

                            if (maxBucketCount > maxBuckets)
                            {
                                string errorMessage = $"Time series for metric '{metric.Name}' on resource '{resourceResult.ResourceId}' contains {maxBucketCount} time buckets, " +
                                                     $"which exceeds the maximum allowed limit of {maxBuckets}. " +
                                                     $"To resolve this issue, either query a smaller time range, " +
                                                     $"increase the interval size (e.g., use PT1H instead of PT5M), " +
                                                     $"or increase the --max-buckets parameter.";

                                context.Response.Status = HttpStatusCode.BadRequest;
                                context.Response.Message = errorMessage;

                                _logger.LogWarning("Bucket limit exceeded. ResourceId: {ResourceId}, MetricName: {MetricName}, BucketCount: {BucketCount}, MaxBuckets: {MaxBuckets}",
                                    resourceResult.ResourceId, metric.Name, maxBucketCount, maxBuckets);

                                return context.Response;
                            }
                        }
                    }
                }
            }

            // Set results
            context.Response.Results = ResponseResult.Create(new(results ?? []), MonitorJsonContext.Default.MetricsBatchQueryCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error querying batch metrics. ResourceGroup: {ResourceGroup}, ResourceType: {ResourceType}, Resources: {Resources}, MetricNames: {MetricNames}.",
                options.ResourceGroup, options.ResourceType, options.Resources, options.MetricNames);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result records
    public sealed record MetricsBatchQueryCommandResult(List<ResourceMetricsResult> Results);
}
