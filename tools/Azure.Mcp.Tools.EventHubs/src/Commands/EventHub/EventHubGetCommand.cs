// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventHubs.Options.EventHub;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.EventHubs.Commands.EventHub;

[CommandMetadata(
    Id = "ab774777-76ac-4e24-ba19-da67254441a9",
    Name = "get",
    Title = "Get Event Hubs from Namespace",
    Description = """
        Get Event Hubs from Azure namespace. This command can either:
        1. List all Event Hubs in a namespace
        2. Get a single Event Hub by name

        When retrieving a single Event Hub or listing multiple Event Hubs, detailed information including
        partition count, settings, and metadata is returned for all Event Hubs.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class EventHubGetCommand(ILogger<EventHubGetCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<EventHubGetOptions, EventHubGetCommand.EventHubGetCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<EventHubGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, EventHubGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(options.Eventhub))
            {
                var eventHub = await _service.GetEventHubAsync(
                    options.Eventhub,
                    options.Namespace,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var results = eventHub != null ? [eventHub] : new List<Models.EventHub>();
                context.Response.Results = ResponseResult.Create(new(results), EventHubsJsonContext.Default.EventHubGetCommandResult);
            }
            else
            {
                var eventHubs = await _service.GetEventHubsAsync(
                    options.Namespace,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(eventHubs ?? []), EventHubsJsonContext.Default.EventHubGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(options.Eventhub))
            {
                _logger.LogError(ex,
                    "Error getting event hub '{EventHub}'. Namespace: {Namespace}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                    options.Eventhub, options.Namespace, options.ResourceGroup, options.Subscription);
            }
            else
            {
                _logger.LogError(ex,
                    "Error listing event hubs. Namespace: {Namespace}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                    options.Namespace, options.ResourceGroup, options.Subscription);
            }
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record EventHubGetCommandResult(List<Models.EventHub> EventHubs);
}
