// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
    private const int MaxBatchResources = 50;

    // Assumed granularity used only to estimate the expected bucket count up front when '--interval' isn't
    // specified. It is never sent to the service -- when '--interval' is omitted, Azure Monitor still selects
    // the actual granularity automatically.
    private const string DefaultValidationInterval = "PT1H";

    private readonly ILogger<MetricsBatchQueryCommand> _logger = logger;
    private readonly IMonitorMetricsService _metricsService = metricsService;

    public override void ValidateOptions(MetricsBatchQueryOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        // '--resources' and '--metric-names' are required options, so the framework already rejects null,
        // empty, or whitespace-only values before this runs. Empty/whitespace-only entries within the
        // comma-delimited list are implicitly ignored (consistent with how other commands handle comma-delimited
        // values); only reject when nothing usable remains after splitting (e.g. the value was just ",").
        string[] resources = options.Resources.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (resources.Length == 0)
        {
            validationResult.Errors.Add($"Invalid format for '--resources'. Provide a comma-separated list of resource names or resource IDs to query (e.g. resource1,resource2).");
        }
        else if (resources.Length > MaxBatchResources)
        {
            validationResult.Errors.Add($"A maximum of {MaxBatchResources} resources can be queried in a single batch request. Provided: {resources.Length}.");
        }

        if (options.MetricNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length == 0)
        {
            validationResult.Errors.Add($"Invalid format for '--metric-names'. Provide a comma-separated list of metric names to query (e.g. CPU,memory).");
        }

        // Validate the start/end time formats up front instead of letting the service throw once the request
        // is already in flight (PostBindOptions guarantees these are always populated by the time this runs).
        bool validStartTime = DateTimeOffset.TryParse(options.StartTime, out var startTime);
        if (!validStartTime)
        {
            validationResult.Errors.Add($"Invalid format for '--start-time': '{options.StartTime}'. Provide a valid date/time (e.g. 2023-01-01T00:00:00Z).");
        }

        bool validEndTime = DateTimeOffset.TryParse(options.EndTime, out var endTime);
        if (!validEndTime)
        {
            validationResult.Errors.Add($"Invalid format for '--end-time': '{options.EndTime}'. Provide a valid date/time (e.g. 2023-01-01T00:00:00Z).");
        }

        // The expected number of time buckets is always derived from the start/end time range up front, so an
        // oversized request is rejected before calling the service -- there's no need to fall back to validating
        // the actual results after the query executes. When '--interval' isn't specified, Azure Monitor selects
        // the granularity automatically, so a representative default is assumed for this estimate only; it is not
        // sent to the service, so the actual query behavior is unaffected.
        bool intervalProvided = !string.IsNullOrWhiteSpace(options.Interval);
        string intervalToValidate = intervalProvided ? options.Interval! : DefaultValidationInterval;

        if (!TryParseIsoDuration(intervalToValidate, out var interval) || interval <= TimeSpan.Zero)
        {
            // The internal default is always a valid duration, so a parse failure here only happens when the
            // user explicitly supplied an invalid '--interval' value.
            validationResult.Errors.Add($"Invalid format for '--interval': '{options.Interval}'. Provide an ISO 8601 duration (e.g. PT1H, PT5M).");
        }
        else if (validStartTime && validEndTime)
        {
            int maxBuckets = options.MaxBuckets ?? 50;
            int expectedBucketCount = (int)Math.Ceiling((endTime - startTime) / interval);

            if (expectedBucketCount > maxBuckets)
            {
                string intervalDescription = intervalProvided
                    ? $"'--interval' of '{options.Interval}'"
                    : $"an assumed default interval of '{DefaultValidationInterval}' (since '--interval' was not specified)";

                validationResult.Errors.Add(
                    $"The requested time range ('--start-time' to '--end-time') combined with {intervalDescription} would produce " +
                    $"approximately {expectedBucketCount} time buckets, which exceeds the maximum allowed limit of {maxBuckets}. " +
                    $"To resolve this issue, either query a smaller time range, specify a larger '--interval' (e.g., PT1H), " +
                    $"or increase the '--max-buckets' parameter.");
            }
        }
    }

    public override void PostBindOptions(MetricsBatchQueryOptions options)
    {
        base.PostBindOptions(options);
        options.StartTime ??= DateTime.UtcNow.AddHours(-24).ToString("o"); // Default to 24 hours ago if not specified
        options.EndTime ??= DateTime.UtcNow.ToString("o"); // Default to now if not specified
    }

    private static bool TryParseIsoDuration(string value, out TimeSpan result)
    {
        try
        {
            result = System.Xml.XmlConvert.ToTimeSpan(value);
            return true;
        }
        catch (FormatException)
        {
            result = TimeSpan.Zero;
            return false;
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MetricsBatchQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            string[] resources = options.Resources.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string[] metricNames = options.MetricNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
