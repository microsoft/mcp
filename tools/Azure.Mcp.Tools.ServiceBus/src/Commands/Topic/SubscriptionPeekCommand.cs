// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ServiceBus.Options.Topic;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ServiceBus.Commands.Topic;

[CommandMetadata(
    Id = "61d32f07-fad6-4e43-9f1e-f0937ce773b3",
    Name = "peek",
    Title = "Peek Messages from Service Bus Topic Subscription",
    Description = """
        Peek messages from a Service Bus subscription without removing them.  Message browsing, or peeking, enables a
        Service Bus client to enumerate all messages in a subscription, for diagnostic and debugging purposes.
        The peek operation returns active, locked, and deferred messages in the subscription.

        Returns message content, properties, and metadata.  Messages remain in the subscription after peeking.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SubscriptionPeekCommand(ILogger<SubscriptionPeekCommand> logger, IServiceBusService serviceBusService)
    : AuthenticatedCommand<SubscriptionPeekOptions, SubscriptionPeekCommand.SubscriptionPeekCommandResult>
{
    private readonly ILogger<SubscriptionPeekCommand> _logger = logger;
    private readonly IServiceBusService _serviceBusService = serviceBusService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SubscriptionPeekOptions options, CancellationToken cancellationToken)
    {
        try
        {

            var messages = await _serviceBusService.PeekSubscriptionMessages(
                options.Namespace,
                options.Topic,
                options.SubscriptionName,
                options.MaxMessages ?? 1,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(messages ?? []), ServiceBusJsonContext.Default.SubscriptionPeekCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error peeking messages from Service Bus topic subscription");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ServiceBusException exception when exception.Reason == ServiceBusFailureReason.MessagingEntityNotFound =>
            $"Subscription not found. Please check the topic and subscription name and try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ServiceBusException sbEx when sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound => HttpStatusCode.NotFound,
        _ => base.GetStatusCode(ex)
    };

    public sealed record SubscriptionPeekCommandResult(List<ServiceBusReceivedMessage> Messages);
}
