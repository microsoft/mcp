// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventHubs.Options.ConsumerGroup;
using Azure.Mcp.Tools.EventHubs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.EventHubs.Commands.ConsumerGroup;

[CommandMetadata(
    Id = "08980fd4-c7c2-41cd-a3c2-eda5303bd458",
    Name = "delete",
    Title = "Delete Event Hubs Consumer Group",
    Description = """
        Delete a Consumer Group. This tool will delete a pre-existing Consumer Group from the specified 
        Event Hub. This tool will remove existing configurations, and is considered to be destructive.

        The tool requires specifying the resource group, Namespace name, Event Hub name, and Consumer
        Group name to identify the Consumer Group to delete.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ConsumerGroupDeleteCommand(ILogger<ConsumerGroupDeleteCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConsumerGroupDeleteOptions, ConsumerGroupDeleteCommand.ConsumerGroupDeleteCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<ConsumerGroupDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConsumerGroupDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _service.DeleteConsumerGroupAsync(
                options.ConsumerGroup,
                options.Eventhub,
                options.Namespace,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(deleted, options.ConsumerGroup, options.Eventhub, options.Namespace, options.ResourceGroup), EventHubsJsonContext.Default.ConsumerGroupDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting consumer group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConsumerGroupDeleteCommandResult(bool Deleted, string ConsumerGroupName, string EventHubName, string NamespaceName, string ResourceGroup);
}
