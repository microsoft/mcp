// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Identity;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.EventHubs.Models;
using Azure.Mcp.Tools.EventHubs.Options;
using Azure.Mcp.Tools.EventHubs.Options.EventHub;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.EventHubs.Commands.EventHub;

public sealed class EventHubGetCommand(ILogger<EventHubGetCommand> logger, IEventHubsService service)
    : BaseEventHubsCommand<EventHubGetOptions>
{
    private const string CommandTitle = "Get Event Hubs from Namespace";
    private readonly IEventHubsService _service = service;
    private readonly ILogger<EventHubGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        """
        Get Event Hubs from Azure namespace. This command can either:
        1. List all Event Hubs in a namespace
        2. Get a single Event Hub by name

        When retrieving a single Event Hub or listing multiple Event Hubs, detailed information including
        partition count, settings, and metadata is returned for all Event Hubs.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = false,
        Idempotent = true,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.NamespaceOption.AsRequired());
        command.Options.Add(EventHubsOptionDefinitions.EventHubOption);
    }

    protected override EventHubGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.NamespaceOption.Name);
        options.EventHub = parseResult.GetValueOrDefault<string>(EventHubsOptionDefinitions.EventHubOption.Name);
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
            bool isSingleEventHubRequest = !string.IsNullOrEmpty(options.EventHub);

            if (isSingleEventHubRequest)
            {
                var eventHub = await _service.GetEventHubAsync(
                    options.EventHub!,
                    options.Namespace!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);

                var results = eventHub != null ? new List<Models.EventHub> { eventHub } : new List<Models.EventHub>();
                context.Response.Results = ResponseResult.Create(new(results), EventHubsJsonContext.Default.EventHubGetCommandResult);
            }
            else
            {
                var eventHubs = await _service.GetEventHubsAsync(
                    options.Namespace!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy);

                context.Response.Results = ResponseResult.Create(new(eventHubs ?? []), EventHubsJsonContext.Default.EventHubGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(options.EventHub))
            {
                _logger.LogError(ex,
                    "Error listing event hubs. Namespace: {Namespace}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                    options.Namespace, options.ResourceGroup, options.Subscription, options);
            }
            else
            {
                _logger.LogError(ex,
                    "Error getting event hub. EventHub: {EventHub}, Namespace: {Namespace}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Options: {@Options}",
                    options.EventHub, options.Namespace, options.ResourceGroup, options.Subscription, options);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        AuthenticationFailedException => HttpStatusCode.Unauthorized,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    internal record EventHubGetCommandResult(List<Models.EventHub> EventHubs);
}
