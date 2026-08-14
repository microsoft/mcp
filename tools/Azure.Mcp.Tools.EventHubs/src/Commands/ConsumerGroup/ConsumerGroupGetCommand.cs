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
    Id = "604fda48-2438-419d-a819-5f9d2f3b21f8",
    Name = "get",
    Title = "Get Event Hubs Consumer Groups",
    Description = """
        Get consumer groups from Azure Event Hub. This command can either:

        1) List all consumer groups in an Event Hub
        2) Get a single consumer group by name

        The EventHub, Namespace, and ResourceGroup parameters are required (for both get and list)
        The Consumer Group parameter is only required for getting a specific consumer-group
        When retrieving a single consumer group and when listing all available consumer groups, return all available metadata on the consumer group.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ConsumerGroupGetCommand(ILogger<ConsumerGroupGetCommand> logger, IEventHubsService service, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ConsumerGroupGetOptions, ConsumerGroupGetCommand.ConsumerGroupGetCommandResult>(subscriptionResolver)
{
    private readonly IEventHubsService _service = service;
    private readonly ILogger<ConsumerGroupGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ConsumerGroupGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(options.ConsumerGroup))
            {
                // Get specific consumer group
                var consumerGroup = await _service.GetConsumerGroupAsync(
                    options.ConsumerGroup,
                    options.Eventhub,
                    options.Namespace,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var singleResult = consumerGroup != null ? [consumerGroup] : new List<Models.ConsumerGroup>();
                context.Response.Results = ResponseResult.Create(new(singleResult), EventHubsJsonContext.Default.ConsumerGroupGetCommandResult);
            }
            else
            {
                // List all consumer groups
                var consumerGroups = await _service.GetConsumerGroupsAsync(
                    options.Eventhub,
                    options.Namespace,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(consumerGroups ?? []), EventHubsJsonContext.Default.ConsumerGroupGetCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting consumer group(s). ConsumerGroup: {ConsumerGroup}, EventHub: {EventHub}, Namespace: {Namespace}, ResourceGroup: {ResourceGroup}.",
                options.ConsumerGroup, options.Eventhub, options.Namespace, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ConsumerGroupGetCommandResult(List<Models.ConsumerGroup> Results);
}
