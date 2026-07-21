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
    Id = "108ffeab-8d37-4c29-98c9-aa99eb8f61c7",
    Name = "delete",
    Title = "Delete Event Hub",
    Description = """
        Delete an Event Hub from an Azure Event Hubs namespace. This operation permanently removes
        the specified Event Hub and all its data. This is a destructive operation.

        The operation is idempotent - if the Event Hub doesn't exist, the command reports success
        with Deleted = false. If the Event Hub is successfully deleted, Deleted = true is returned.
        Warning: This operation cannot be undone. All messages and consumer groups in the Event Hub
        will be permanently deleted.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class EventHubDeleteCommand(ILogger<EventHubDeleteCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<EventHubDeleteOptions, EventHubDeleteCommand.EventHubDeleteCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<EventHubDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, EventHubDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _service.DeleteEventHubAsync(
                options.Eventhub,
                options.Namespace,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(deleted, options.Eventhub),
                EventHubsJsonContext.Default.EventHubDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting event hub. EventHub: {EventHub}, Namespace: {Namespace}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}.",
                options.Eventhub, options.Namespace, options.ResourceGroup, options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record EventHubDeleteCommandResult(bool Deleted, string EventHubName);
}
