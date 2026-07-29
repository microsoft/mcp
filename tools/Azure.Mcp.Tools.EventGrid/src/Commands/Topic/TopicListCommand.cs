// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.EventGrid.Models;
using Azure.Mcp.Tools.EventGrid.Options.Topic;
using Azure.Mcp.Tools.EventGrid.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.EventGrid.Commands.Topic;

[CommandMetadata(
    Id = "42390294-2856-4980-a057-095c91355650",
    Name = "list",
    Title = "List Event Grid Topics",
    Description = "List Event Grid topics in an Azure subscription or resource group. Returns topic names, endpoints, locations, and provisioning status.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TopicListCommand(ILogger<TopicListCommand> logger, IEventGridService eventGridService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TopicListOptions, TopicListCommand.TopicListCommandResult>(subscriptionResolver)
{
    private readonly ILogger<TopicListCommand> _logger = logger;
    private readonly IEventGridService _eventGridService = eventGridService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TopicListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var topics = await _eventGridService.GetTopicsAsync(
                options.Subscription!,
                options.ResourceGroup,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(topics ?? []), EventGridJsonContext.Default.TopicListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Event Grid topics. Subscription: {Subscription}.", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TopicListCommandResult(List<EventGridTopicInfo> Topics);
}
