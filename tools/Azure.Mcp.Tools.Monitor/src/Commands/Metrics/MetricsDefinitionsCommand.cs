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
/// Command for listing Azure Monitor metric definitions
/// </summary>
[CommandMetadata(
    Id = "d3bf37ed-5f2e-448d-a16e-73140ef908c2",
    Name = "definitions",
    Title = "List Azure Monitor Metric Definitions",
    Description = "List available metric definitions for an Azure resource. Returns metadata about the metrics available for the resource.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class MetricsDefinitionsCommand(ILogger<MetricsDefinitionsCommand> logger, IMonitorMetricsService metricsService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<MetricsDefinitionsOptions, MetricsDefinitionsCommand.MetricsDefinitionsCommandResult>(subscriptionResolver)
{
    private readonly ILogger<MetricsDefinitionsCommand> _logger = logger;
    private readonly IMonitorMetricsService _metricsService = metricsService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, MetricsDefinitionsOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation with required parameters
            var allResults = await _metricsService.ListMetricDefinitionsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.ResourceType,
                options.Resource!,
                options.MetricNamespace,
                options.SearchString,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            if (allResults?.Count > 0)
            {
                // Apply limiting and determine status
                var totalCount = allResults.Count;
                var limitedResults = allResults.Take(options.Limit).ToList();
                var isTruncated = totalCount > options.Limit;

                string status;
                if (isTruncated)
                {
                    status = $"Results truncated to {options.Limit} of {totalCount} metric definitions. Use --search-string to filter results for more specific metrics or increase --limit to see more results.";
                }
                else
                {
                    status = $"All {totalCount} metric definitions returned.";
                }

                // Set response message and results
                context.Response.Message = status;
                context.Response.Results = ResponseResult.Create(new(limitedResults, status), MonitorJsonContext.Default.MetricsDefinitionsCommandResult);
            }
            else
            {
                context.Response.Results = null;
            }
        }
        catch (Exception ex)
        {            // Log error with all relevant context
            _logger.LogError(ex,
                "Error listing metric definitions. ResourceGroup: {ResourceGroup}, ResourceType: {ResourceType}, ResourceName: {ResourceName}, MetricNamespace: {MetricNamespace}.",
                options.ResourceGroup, options.ResourceType, options.Resource, options.MetricNamespace);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result record
    public sealed record MetricsDefinitionsCommandResult(List<MetricDefinition> Results, string Status);
}
