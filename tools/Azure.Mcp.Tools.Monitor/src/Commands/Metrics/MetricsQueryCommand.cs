// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
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

    // Define options from OptionDefinitions
    private readonly Option<string> _metricNamesOption = MonitorOptionDefinitions.Metrics.MetricNames;
    private readonly Option<string> _startTimeOption = MonitorOptionDefinitions.Metrics.StartTime;
    private readonly Option<string> _endTimeOption = MonitorOptionDefinitions.Metrics.EndTime;
    private readonly Option<string> _intervalOption = MonitorOptionDefinitions.Metrics.Interval;
    private readonly Option<string> _aggregationOption = MonitorOptionDefinitions.Metrics.Aggregation;
    private readonly Option<string> _filterOption = MonitorOptionDefinitions.Metrics.Filter;
    private readonly Option<string> _metricNamespaceOption = MonitorOptionDefinitions.Metrics.MetricNamespace;
    private readonly Option<int> _maxBucketsOption = MonitorOptionDefinitions.Metrics.MaxBuckets;

    public override string Name => "query";

    public override string Description =>
            $"""
                Query Azure Monitor metrics for a resource. Returns time series data for the specified metrics.
                """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_metricNamesOption);
        command.Options.Add(_startTimeOption);
        command.Options.Add(_endTimeOption);
        command.Options.Add(_intervalOption);
        command.Options.Add(_aggregationOption);
        command.Options.Add(_filterOption);
        command.Options.Add(_metricNamespaceOption);
        command.Options.Add(_maxBucketsOption);
    }

    protected override MetricsQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.MetricNames = parseResult.GetValue(_metricNamesOption);
        options.StartTime = parseResult.GetValue(_startTimeOption);
        options.EndTime = parseResult.GetValue(_endTimeOption);
        options.Interval = parseResult.GetValue(_intervalOption);
        options.Aggregation = parseResult.GetValue(_aggregationOption);
        options.Filter = parseResult.GetValue(_filterOption);
        options.MetricNamespace = parseResult.GetValue(_metricNamespaceOption);
        options.MaxBuckets = parseResult.GetValue(_maxBucketsOption);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);

        if (result.IsValid)
        {
            commandResult.TryGetValue(_metricNamesOption, out string? metricNamesValue);

            if (string.IsNullOrWhiteSpace(metricNamesValue))
            {
                result.IsValid = false;
                result.ErrorMessage = $"Invalid format for {_metricNamesOption.Name}. Provide a comma-separated list of metric names to query (e.g. CPU,memory).";

                if (commandResponse != null)
                {
                    commandResponse.Status = 400;
                    commandResponse.Message = result.ErrorMessage!;
                }
            }
            else
            {
                // Validate the metric names
                string[] metricNames = [.. metricNamesValue.Split(',').Select(t => t.Trim())];

                if (metricNames.Length == 0 || metricNames.Any(s => string.IsNullOrWhiteSpace(s)))
                {
                    result.IsValid = false;
                    result.ErrorMessage = $"Invalid format for {_metricNamesOption.Name}. Provide a comma-separated list of metric names to query (e.g. CPU,memory).";

                    if (commandResponse != null)
                    {
                        commandResponse.Status = 400;
                        commandResponse.Message = result.ErrorMessage!;
                    }
                }
            }
        }
        return result;
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

                            context.Response.Status = 400;
                            context.Response.Message = errorMessage;

                            _logger.LogWarning("Bucket limit exceeded. Metric: {MetricName}, BucketCount: {BucketCount}, MaxBuckets: {MaxBuckets}",
                                metric.Name, maxBucketCount, maxBuckets);

                            return context.Response;
                        }
                    }
                }
            }

            // Set results
            context.Response.Results = results?.Count > 0 ?
                ResponseResult.Create(
                    new MetricsQueryCommandResult(results),
                    MonitorJsonContext.Default.MetricsQueryCommandResult) :
                null;
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
