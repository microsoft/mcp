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
    Id = "859871ba-b8dc-439c-a607-11b0d89f5112",
    Name = "update",
    Title = "Create or Update Event Hubs Consumer Group",
    Description = """
        Create or Update a Consumer Group. This tool will either create a Consumer Group resource 
        or update a pre-existing Consumer Group resource within the specified Event Hub, depending 
        on whether or not the specified Consumer Group already exists. This tool may modify existing 
        configurations, and is considered to be destructive. 

        The tool requires specifying the resource group, namespace name, event hub name, and consumer 
        group name. Optionally, you can provide user metadata for the consumer group.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ConsumerGroupUpdateCommand(ILogger<ConsumerGroupUpdateCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConsumerGroupUpdateOptions, ConsumerGroupUpdateCommand.ConsumerGroupUpdateCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<ConsumerGroupUpdateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConsumerGroupUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var consumerGroup = await _service.CreateOrUpdateConsumerGroupAsync(
                options.ConsumerGroup,
                options.Eventhub,
                options.Namespace,
                options.ResourceGroup,
                options.Subscription!,
                options.UserMetadata,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(consumerGroup), EventHubsJsonContext.Default.ConsumerGroupUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating consumer group");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConsumerGroupUpdateCommandResult(Models.ConsumerGroup ConsumerGroup);
}
