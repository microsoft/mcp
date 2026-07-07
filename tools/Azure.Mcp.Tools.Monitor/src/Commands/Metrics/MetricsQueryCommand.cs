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
/// Command for querying Azure Monitor metrics
/// </summary>
[CommandMetadata(
    Id = "6e86ef31-04e1-4cec-8bda-5292d4bc3ad8",
    Name = "query",
    Title = "Query Azure Monitor Metrics",
    Description = "Query Azure Monitor metrics for a resource. Returns time series data for the specified metrics.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MetricsQueryCommand(ILogger<MetricsQueryCommand> logger, IMonitorMetricsService metricsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MetricsQueryOptions, MetricsQueryCommand.MetricsQueryCommandResult>(subscriptionResolver)
{
    private readonly ILogger<MetricsQueryCommand> _logger = logger;
    private readonly IMonitorMetricsService _metricsService = metricsService;

    public override void ValidateOptions(MetricsQueryOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.MetricNames))
        {
            validationResult.Errors.Add($"Invalid format for '--metric-names'. Provide a comma-separated list of metric names to query (e.g. CPU,memory).");
        }
        else
        {
            // Validate the metric names
            string[] metricNames = [.. options.MetricNames.Split(',').Select(t => t.Trim())];

            if (metricNames.Length == 0 || metricNames.Any(s => string.IsNullOrWhiteSpace(s)))
            {
                validationResult.Errors.Add($"Invalid format for '--metric-names'. Provide a comma-separated list of metric names to query (e.g. CPU,memory).");
            }
        }
    }

    public override void PostBindOptions(MetricsQueryOptions options)
    {
        base.PostBindOptions(options);
        options.StartTime ??= DateTime.UtcNow.AddHours(-24).ToString("o"); // Default to 24 hours ago if not specified
        options.EndTime ??= DateTime.UtcNow.ToString("o"); // Default to now if not specified
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MetricsQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string[] metricNames = [.. options.MetricNames.Split(',').Select(t => t.Trim())];

            var results = await _metricsService.QueryMetricsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.ResourceType,
                options.Resource,
                options.MetricNamespace,
                metricNames,
                options.StartTime,
                options.EndTime,
                options.Interval,
                options.Aggregation,
                options.Filter,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Validate bucket count limit
            if (results?.Count > 0)
            {
                int maxBuckets = options.MaxBuckets ?? 50; // Use provided value or default to 50

                foreach (var metric in results)
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
                            string errorMessage = $"Time series for metric '{metric.Name}' contains {maxBucketCount} time buckets, " +
                                                 $"which exceeds the maximum allowed limit of {maxBuckets}. " +
                                                 $"To resolve this issue, either query a smaller time range, " +
                                                 $"increase the interval size (e.g., use PT1H instead of PT5M), " +
                                                 $"or increase the --max-buckets parameter.";

                            context.Response.Status = HttpStatusCode.BadRequest;
                            context.Response.Message = errorMessage;

                            _logger.LogWarning("Bucket limit exceeded. Metric: {MetricName}, BucketCount: {BucketCount}, MaxBuckets: {MaxBuckets}",
                                metric.Name, maxBucketCount, maxBuckets);

                            return context.Response;
                        }
                    }
                }
            }

            // Set results
            context.Response.Results = ResponseResult.Create(new(results ?? []), MonitorJsonContext.Default.MetricsQueryCommandResult);
        }
        catch (Exception ex)
        {            // Log error with all relevant context
            _logger.LogError(ex,
                "Error querying metrics. ResourceGroup: {ResourceGroup}, ResourceType: {ResourceType}, ResourceName: {ResourceName}, MetricNames: {@MetricNames}.",
                options.ResourceGroup, options.ResourceType, options.Resource, options.MetricNames);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result records
    public sealed record MetricsQueryCommandResult(List<MetricResult> Results);
}
