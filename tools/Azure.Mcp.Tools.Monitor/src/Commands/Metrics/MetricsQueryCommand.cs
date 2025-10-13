// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.Metrics;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.Metrics;

/// <summary>
/// Command for querying Azure Monitor metrics
/// </summary>
public sealed class MetricsQueryCommand(ILogger<MetricsQueryCommand> logger)
    : BaseMetricsCommand<MetricsQueryOptions>
{
    private const string CommandTitle = "Query Azure Monitor Metrics";
    private readonly ILogger<MetricsQueryCommand> _logger = logger;

    public override string Name => "query";

    public override string Description =>
        $"""
        Query Azure Monitor metrics for a resource. Returns time series data for the specified metrics.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(MonitorOptionDefinitions.Metrics.MetricNames);
        command.Options.Add(MonitorOptionDefinitions.Metrics.StartTime);
        command.Options.Add(MonitorOptionDefinitions.Metrics.EndTime);
        command.Options.Add(MonitorOptionDefinitions.Metrics.Interval);
        command.Options.Add(MonitorOptionDefinitions.Metrics.Aggregation);
        command.Options.Add(MonitorOptionDefinitions.Metrics.Filter);
        command.Options.Add(MonitorOptionDefinitions.Metrics.MetricNamespace);
        command.Options.Add(MonitorOptionDefinitions.Metrics.MaxBuckets);
        command.Validators.Add(commandResult =>
        {
            commandResult.TryGetValue(MonitorOptionDefinitions.Metrics.MetricNames, out string? metricNamesValue);

            if (string.IsNullOrWhiteSpace(metricNamesValue))
            {
                commandResult.AddError($"Invalid format for {MonitorOptionDefinitions.Metrics.MetricNames.Name}. Provide a comma-separated list of metric names to query (e.g. CPU,memory).");
            }
            else
            {
                // Validate the metric names
                string[] metricNames = [.. metricNamesValue.Split(',').Select(t => t.Trim())];

                if (metricNames.Length == 0 || metricNames.Any(s => string.IsNullOrWhiteSpace(s)))
                {
                    commandResult.AddError($"Invalid format for {MonitorOptionDefinitions.Metrics.MetricNames.Name}. Provide a comma-separated list of metric names to query (e.g. CPU,memory).");
                }
            }
        });
    }

    protected override MetricsQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.MetricNames = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.MetricNames.Name);
        options.StartTime = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.StartTime.Name);
        options.EndTime = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.EndTime.Name);
        options.Interval = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.Interval.Name);
        options.Aggregation = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.Aggregation.Name);
        options.Filter = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.Filter.Name);
        options.MetricNamespace = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.MetricNamespace.Name);
        options.MaxBuckets = parseResult.GetValueOrDefault<int>(MonitorOptionDefinitions.Metrics.MaxBuckets.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);

        try
        {
            string[] metricNames = [.. options.MetricNames!.Split(',').Select(t => t.Trim())];

            // Get the metrics service from DI
            var service = context.GetService<IMonitorMetricsService>();

            // Call the metrics service method directly
            var results = await service.QueryMetricsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.ResourceType,
                options.ResourceName!,
                options.MetricNamespace!,
                metricNames,
                options.StartTime,
                options.EndTime,
                options.Interval,
                options.Aggregation,
                options.Filter,
                options.Tenant,
                options.RetryPolicy);

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
                "Error querying metrics. ResourceGroup: {ResourceGroup}, ResourceType: {ResourceType}, ResourceName: {ResourceName}, MetricNames: {@MetricNames}, Options: {@Options}",
                options.ResourceGroup, options.ResourceType, options.ResourceName, options.MetricNames, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result records
    internal record MetricsQueryCommandResult(List<MetricResult> Results);
}
