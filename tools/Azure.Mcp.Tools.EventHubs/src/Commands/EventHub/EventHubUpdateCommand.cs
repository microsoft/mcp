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
    Id = "1df73670-9de5-4d4b-bdd8-9d2d9e16f732",
    Name = "update",
    Title = "Create or Update Event Hub",
    Description = """
        Create or update an Event Hub within an Azure Event Hubs namespace. This command can either:
        1. Create a new Event Hub if it doesn't exist
        2. Update an existing Event Hub's configuration

        You can configure:
        - Partition count (number of partitions for parallel processing)
        - Message retention time (how long messages are retained in hours)

        Note: Some properties like partition count cannot be changed after creation.
        This is a potentially long-running operation that waits for completion.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class EventHubUpdateCommand(ILogger<EventHubUpdateCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<EventHubUpdateOptions, EventHubUpdateCommand.EventHubUpdateCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<EventHubUpdateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, EventHubUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var eventHub = await _service.CreateOrUpdateEventHubAsync(
                options.Eventhub,
                options.Namespace,
                options.ResourceGroup,
                options.Subscription!,
                options.PartitionCount,
                options.MessageRetentionInHours,
                options.Status,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(eventHub),
                EventHubsJsonContext.Default.EventHubUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating event hub. EventHub: {EventHub}, Namespace: {Namespace}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.Eventhub, options.Namespace, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record EventHubUpdateCommandResult(Models.EventHub EventHub);
}
