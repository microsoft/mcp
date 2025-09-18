// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Options;
using Azure.Mcp.Tools.ServiceBus.Options.Queue;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ServiceBus.Commands.Queue;

public sealed class QueueDetailsCommand(ILogger<QueueDetailsCommand> logger) : SubscriptionCommand<BaseQueueOptions>
{
    private const string CommandTitle = "Get Service Bus Queue Details";
    private readonly ILogger<QueueDetailsCommand> _logger = logger;
    public override string Name => "details";

    public override string Description =>
        """
        Get details about a Service Bus queue. Returns queue properties and runtime information. Properties returned include
        lock duration, max message size, queue size, creation date, status, current message counts, etc.

        Required arguments:
        - namespace: The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)
        - queue: Queue name to get details and runtime information for.
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
        command.Options.Add(ServiceBusOptionDefinitions.Namespace);
        command.Options.Add(ServiceBusOptionDefinitions.Queue);
    }

    protected override BaseQueueOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(ServiceBusOptionDefinitions.Queue.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(ServiceBusOptionDefinitions.Namespace.Name);
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
            var details = await service.GetQueueDetails(
                options.Namespace!,
                options.Name!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(details), ServiceBusJsonContext.Default.QueueDetailsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Service Bus queue details");
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
    internal record QueueDetailsCommandResult(QueueDetails QueueDetails);
}
