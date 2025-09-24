// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
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
        // Optional --start-time and --end-time (ISO 8601)
        var startTime = new Option<string>("--start-time") { Description = "Optional start time in ISO 8601 UTC (e.g., 2025-01-01T00:00:00Z). Defaults to 1 hour ago." };
        var endTime = new Option<string>("--end-time") { Description = "Optional end time in ISO 8601 UTC (e.g., 2025-01-01T01:00:00Z). Defaults to now." };
        command.Options.Add(startTime);
        command.Options.Add(endTime);
    }

    protected override AppTraceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);

        string? startRaw = parseResult.GetValueOrDefault<string>("start-time");
        string? endRaw = parseResult.GetValueOrDefault<string>("end-time");
        if (DateTime.TryParse(startRaw, out var startUtc))
        {
            options.StartTimeUtc = startUtc.ToUniversalTime();
        }
        if (DateTime.TryParse(endRaw, out var endUtc))
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
            var service = context.GetService<IApplicationInsightsService>();
            var traces = await service.GetAppTracesAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                options.StartTimeUtc,
                options.EndTimeUtc);

            context.Response.Results = traces?.Any() == true ?
                ResponseResult.Create(new AppTraceListCommandResult(traces), ApplicationInsightsJsonContext.Default.AppTraceListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Application Insights trace metadata.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record AppTraceListCommandResult(IEnumerable<JsonNode> Traces);
}
