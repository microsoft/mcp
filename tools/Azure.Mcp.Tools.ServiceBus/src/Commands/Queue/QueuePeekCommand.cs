// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ServiceBus.Options;
using Azure.Mcp.Tools.ServiceBus.Options.Queue;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ServiceBus.Commands.Queue;

public sealed class QueuePeekCommand(ILogger<QueuePeekCommand> logger) : SubscriptionCommand<QueuePeekOptions>
{
    private const string CommandTitle = "Peek Messages from Service Bus Queue";
    private readonly Option<string> _queueOption = ServiceBusOptionDefinitions.Queue;
    private readonly Option<int> _maxMessagesOption = ServiceBusOptionDefinitions.MaxMessages;
    private readonly Option<string> _namespaceOption = ServiceBusOptionDefinitions.Namespace;
    private readonly ILogger<QueuePeekCommand> _logger = logger;
    public override string Name => "peek";

    public override string Description =>
        """
        Peek messages from a Service Bus queue without removing them.  Message browsing, or peeking, enables a
        Service Bus client to enumerate all messages in a queue, for diagnostic and debugging purposes.
        The peek operation returns active, locked, deferred, and scheduled messages in the queue.

        Returns message content, properties, and metadata.  Messages remain in the queue after peeking.

        Required arguments:
        - namespace: The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)
        - queue: Queue name to peek messages from
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_namespaceOption);
        command.Options.Add(_queueOption);
        command.Options.Add(_maxMessagesOption);
    }

    protected override QueuePeekOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault(_queueOption);
        options.Namespace = parseResult.GetValueOrDefault(_namespaceOption);
        options.MaxMessages = parseResult.GetValueOrDefault(_maxMessagesOption);
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
            var service = context.GetService<IServiceBusService>();
            var messages = await service.PeekQueueMessages(
                options.Namespace!,
                options.Name!,
                options.MaxMessages ?? 1,
                options.Tenant,
                options.RetryPolicy);

            var peekedMessages = messages ?? new List<ServiceBusReceivedMessage>();

            context.Response.Results = ResponseResult.Create(
                new QueuePeekCommandResult(peekedMessages),
                ServiceBusJsonContext.Default.QueuePeekCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error peeking messages from Service Bus queue");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ServiceBusException exception when exception.Reason == ServiceBusFailureReason.MessagingEntityNotFound =>
            $"Queue not found. Please check the queue name and try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ServiceBusException sbEx when sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound => 404,
        _ => base.GetStatusCode(ex)
    };

    internal record QueuePeekCommandResult(List<ServiceBusReceivedMessage> Messages);
}
