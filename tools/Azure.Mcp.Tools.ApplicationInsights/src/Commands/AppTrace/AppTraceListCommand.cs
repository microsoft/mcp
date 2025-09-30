// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.Mcp.Tools.ApplicationInsights.Options;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Extensions.Logging;

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
        """
        List Application Insights trace metadata (component identifiers and time window) in a subscription. Optionally filter by resource group when --resource-group is provided.
        This is an initial implementation that returns component metadata and a requested time window; future versions may return detailed trace/span data.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, LocalRequired = false, OpenWorld = false, Secret = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(ApplicationInsightsOptionDefinitions.ResourceName.AsOptional());
        command.Options.Add(ApplicationInsightsOptionDefinitions.ResourceId.AsOptional());
        command.Options.Add(ApplicationInsightsOptionDefinitions.Table.AsRequired());
        command.Options.Add(ApplicationInsightsOptionDefinitions.Filters.AsOptional());
        command.Options.Add(ApplicationInsightsOptionDefinitions.StartTime.AsOptional());
        command.Options.Add(ApplicationInsightsOptionDefinitions.EndTime.AsOptional());
    }

    protected override AppTraceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

        options.ResourceName ??= parseResult.GetValueOrDefault<string>(ApplicationInsightsOptionDefinitions.ResourceName.Name);
        options.ResourceId ??= parseResult.GetValueOrDefault<string>(ApplicationInsightsOptionDefinitions.ResourceId.Name);

        options.Table = parseResult.GetValue<string>(ApplicationInsightsOptionDefinitions.Table.Name);
        options.Filters = parseResult.GetValueOrDefault<string[]>(ApplicationInsightsOptionDefinitions.Filters.Name) ?? [];

        string? startRaw = parseResult.GetValueOrDefault<string>(ApplicationInsightsOptionDefinitions.StartTime.Name);
        string? endRaw = parseResult.GetValueOrDefault<string>(ApplicationInsightsOptionDefinitions.EndTime.Name);

        // How to leverage default values from Option definitions?
        options.EndTimeUtc = DateTime.UtcNow;
        options.StartTimeUtc = options.EndTimeUtc.Value.AddHours(-24);

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

        var options = BindOptions(parseResult);
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
