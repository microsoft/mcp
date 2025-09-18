// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Monitor.Models;
using Azure.Mcp.Tools.Monitor.Options;
using Azure.Mcp.Tools.Monitor.Options.Metrics;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.Metrics;

/// <summary>
/// Command for listing Azure Monitor metric definitions
/// </summary>
public sealed class MetricsDefinitionsCommand(ILogger<MetricsDefinitionsCommand> logger)
    : BaseMetricsCommand<MetricsDefinitionsOptions>
{
    private const string CommandTitle = "List Azure Monitor Metric Definitions";
    private readonly ILogger<MetricsDefinitionsCommand> _logger = logger;

    public override string Name => "definitions";

    public override string Description =>
            $"""
                List available metric definitions for an Azure resource. Returns metadata about the metrics available for the resource.
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
        command.Options.Add(MonitorOptionDefinitions.Metrics.MetricNamespace.AsOptional());
        command.Options.Add(MonitorOptionDefinitions.Metrics.SearchString);
        command.Options.Add(MonitorOptionDefinitions.Metrics.DefinitionsLimit);
    }

    protected override MetricsDefinitionsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.MetricNamespace = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.MetricNamespace.Name);
        options.SearchString = parseResult.GetValueOrDefault<string>(MonitorOptionDefinitions.Metrics.SearchString.Name);
        options.Limit = parseResult.GetValueOrDefault<int>(MonitorOptionDefinitions.Metrics.DefinitionsLimit.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Get the metrics service from DI
            var service = context.GetService<IMonitorMetricsService>();
            // Call service operation with required parameters
            var allResults = await service.ListMetricDefinitionsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.ResourceType,
                options.ResourceName!,
                options.MetricNamespace,
                options.SearchString,
                options.Tenant,
                options.RetryPolicy);

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
                "Error listing metric definitions. ResourceGroup: {ResourceGroup}, ResourceType: {ResourceType}, ResourceName: {ResourceName}, MetricNamespace: {MetricNamespace}, Options: {@Options}",
                options.ResourceGroup, options.ResourceType, options.ResourceName, options.MetricNamespace, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result record
    internal record MetricsDefinitionsCommandResult(List<MetricDefinition> Results, string Status);
}
