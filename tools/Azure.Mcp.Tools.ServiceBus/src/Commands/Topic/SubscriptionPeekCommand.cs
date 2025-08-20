// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.ServiceBus.Options;
using Azure.Mcp.Tools.ServiceBus.Options.Topic;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ServiceBus.Commands.Topic;

public sealed class SubscriptionPeekCommand(ILogger<SubscriptionPeekCommand> logger) : SubscriptionCommand<SubscriptionPeekOptions>
{
    private const string CommandTitle = "Peek Messages from Service Bus Topic Subscription";
    private readonly Option<string> _topicOption = ServiceBusOptionDefinitions.Topic;
    private readonly Option<string> _subscriptionNameOption = ServiceBusOptionDefinitions.Subscription;
    private readonly Option<int> _maxMessagesOption = ServiceBusOptionDefinitions.MaxMessages;
    private readonly Option<string> _namespaceOption = ServiceBusOptionDefinitions.Namespace;
    private readonly ILogger<SubscriptionPeekCommand> _logger = logger;
    public override string Name => "peek";

    public override string Description =>
        """
        Peek messages from a Service Bus subscription without removing them.  Message browsing, or peeking, enables a
        Service Bus client to enumerate all messages in a subscription, for diagnostic and debugging purposes.
        The peek operation returns active, locked, and deferred messages in the subscription.

        Returns message content, properties, and metadata.  Messages remain in the subscription after peeking.

        Required arguments:
        - namespace: The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)
        - topic: Topic name containing the subscription
        - subscription-name: Subscription name to peek messages from
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_namespaceOption);
        command.AddOption(_topicOption);
        command.AddOption(_subscriptionNameOption);
        command.AddOption(_maxMessagesOption);
    }

    protected override SubscriptionPeekOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SubscriptionName = parseResult.GetValueForOption(_subscriptionNameOption);
        options.TopicName = parseResult.GetValueForOption(_topicOption);
        options.Namespace = parseResult.GetValueForOption(_namespaceOption);
        options.MaxMessages = parseResult.GetValueForOption(_maxMessagesOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<IServiceBusService>();
            var messages = await service.PeekSubscriptionMessages(
                options.Namespace!,
                options.TopicName!,
                options.SubscriptionName!,
                options.MaxMessages ?? 1,
                options.Tenant,
                options.RetryPolicy);

            var peekedMessages = messages ?? new List<ServiceBusReceivedMessage>();

            context.Response.Results = ResponseResult.Create(
                new SubscriptionPeekCommandResult(peekedMessages),
                ServiceBusJsonContext.Default.SubscriptionPeekCommandResult);
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

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ServiceBusException sbEx when sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound => 404,
        _ => base.GetStatusCode(ex)
    };

    internal record SubscriptionPeekCommandResult(List<ServiceBusReceivedMessage> Messages);
}
