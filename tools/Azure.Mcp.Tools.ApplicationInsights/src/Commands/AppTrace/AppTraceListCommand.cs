// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.Mcp.Tools.ApplicationInsights.Options;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.Logging;
using static Azure.Mcp.Tools.ApplicationInsights.Options.ApplicationInsightsOptionDefinitions;

namespace Azure.Mcp.Tools.ApplicationInsights.Commands.AppTrace;

/// <summary>
/// Command to list Application Insights trace metadata for components in a subscription or resource group.
/// </summary>
public sealed class AppTraceListCommand(ILogger<AppTraceListCommand> logger) : SubscriptionCommand<AppTraceListOptions>()
{
    private const string CommandTitle = "List Application Insights Trace Metadata";
    private readonly ILogger<AppTraceListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $$"""
        List the most relevant traces from an Application Insights table.

        This tool is useful for correlating errors and dependencies to specific transactions in an application.

        Returns a list of traceIds and spanIds that can be further explored for each operation.

        Example usage:
        Filter to dependency failures
        "table": "dependencies",
        "filters": ["success=\"false\""]
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, LocalRequired = false, OpenWorld = false, Secret = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(ResourceName);
        command.Options.Add(ResourceId);
        command.Options.Add(Table);
        command.Options.Add(Filters);
        command.Options.Add(StartTime);
        command.Options.Add(EndTime);
    }

    private ValidationResult OnExecuting(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        ValidationResult result = new();

        // Either resourceName or resourceId must be provided
        string? resourceName = commandResult.GetValueOrDefault(ResourceName);
        string? resourceId = commandResult.GetValueOrDefault(ResourceId);
        if (string.IsNullOrEmpty(resourceName) && string.IsNullOrEmpty(resourceId))
        {
            result.Errors.Add($"Either --{ResourceNameName} or --{ResourceIdName} must be provided.");
        }

        // Validate time range
        if (!DateTime.TryParse(commandResult.GetValueOrDefault(StartTime), out DateTime startTime) ||
            !DateTime.TryParse(commandResult.GetValueOrDefault(EndTime), out DateTime endTime) ||
            startTime >= endTime)
        {
            result.Errors.Add($"Invalid time range specified. Ensure that --{StartTimeName} is before --{EndTimeName} and that both are valid dates in ISO format.");
        }

        // Validate table name
        string? table = commandResult.GetValueOrDefault(Table);

        if (!string.Equals(table, "exceptions", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(table, "dependencies", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(table, "availabilityResults", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(table, "requests", StringComparison.OrdinalIgnoreCase))
        {
            result.Errors.Add($"Invalid table specified. Valid options are: exceptions, dependencies, availabilityResults, requests.");
        }

        if (!result.IsValid && commandResponse != null)
        {
            commandResponse.Status = HttpStatusCode.BadRequest;
            commandResponse.Message = string.Join('\n', result.Errors);
        }

        return result;
    }

    protected override AppTraceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault(OptionDefinitions.Common.ResourceGroup);

        options.ResourceName ??= parseResult.GetValueOrDefault(ResourceName);
        options.ResourceId ??= parseResult.GetValueOrDefault(ResourceId);

        options.Table = parseResult.GetValue(Table);
        options.Filters = parseResult.GetValueOrDefault(Filters) ?? [];

        string? startRaw = parseResult.GetValueOrDefault(StartTime);
        string? endRaw = parseResult.GetValueOrDefault(EndTime);

        if (DateTime.TryParse(startRaw, out DateTime startUtc))
        {
            options.StartTimeUtc = startUtc.ToUniversalTime();
        }

        if (DateTime.TryParse(endRaw, out DateTime endUtc))
        {
            options.EndTimeUtc = endUtc.ToUniversalTime();
        }

        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        if (!OnExecuting(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        AppTraceListOptions options = BindOptions(parseResult);
        try
        {
            IApplicationInsightsService service = context.GetService<IApplicationInsightsService>();
            AppListTraceResult traces = await service.ListDistributedTracesAsync(
                subscription: options.Subscription!,
                resourceGroup: options.ResourceGroup,
                resourceName: options.ResourceName,
                resourceId: options.ResourceId,
                filters: options.Filters,
                table: options.Table!,
                startTime: options.StartTimeUtc!.Value,
                endTime: options.EndTimeUtc!.Value,
                tenant: options.Tenant,
                options.RetryPolicy);

            context.Response.Results = traces is not null ?
                ResponseResult.Create(new AppTraceListCommandResult(traces), ApplicationInsightsJsonContext.Default.AppTraceListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error in {Name}. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, ResourceName: {ResourceName}, Options: {@Options}",
                Name, options.Subscription, options.ResourceGroup, options.ResourceName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record AppTraceListCommandResult(AppListTraceResult? Traces);
}
