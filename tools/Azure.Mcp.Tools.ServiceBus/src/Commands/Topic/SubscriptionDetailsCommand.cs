// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Options;
using Azure.Mcp.Tools.ServiceBus.Options.Topic;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ServiceBus.Commands.Topic;

public sealed class SubscriptionDetailsCommand(ILogger<SubscriptionDetailsCommand> logger) : SubscriptionCommand<SubscriptionDetailsOptions>
{
    private const string CommandTitle = "Get Service Bus Topic Subscription Details";
    private readonly Option<string> _namespaceOption = ServiceBusOptionDefinitions.Namespace;
    private readonly Option<string> _topicOption = ServiceBusOptionDefinitions.Topic;
    private readonly Option<string> _subscriptionNameOption = ServiceBusOptionDefinitions.Subscription;
    private readonly ILogger<SubscriptionDetailsCommand> _logger = logger;
    public override string Name => "details";

    public override string Description =>
        """
        Get details about a Service Bus subscription. Returns subscription runtime properties including message counts, delivery settings, and other metadata.

        Required arguments:
        - namespace: The fully qualified Service Bus namespace host name. (This is usually in the form <namespace>.servicebus.windows.net)
        - topic: Topic name containing the subscription
        - subscription-name: Name of the subscription to get details for
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_namespaceOption);
        command.AddOption(_topicOption);
        command.AddOption(_subscriptionNameOption);
    }

    protected override SubscriptionDetailsOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Namespace = parseResult.GetValueForOption(_namespaceOption);
        options.TopicName = parseResult.GetValueForOption(_topicOption);
        options.SubscriptionName = parseResult.GetValueForOption(_subscriptionNameOption);
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
            var details = await service.GetSubscriptionDetails(
                options.Namespace!,
                options.TopicName!,
                options.SubscriptionName!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new SubscriptionDetailsCommandResult(details),
                ServiceBusJsonContext.Default.SubscriptionDetailsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Service Bus subscription details");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ServiceBusException exception when exception.Reason == ServiceBusFailureReason.MessagingEntityNotFound =>
            $"Topic or subscription not found. Please check the topic and subscription names and try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ServiceBusException sbEx when sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound => 404,
        _ => base.GetStatusCode(ex)
    };

    internal record SubscriptionDetailsCommandResult(SubscriptionDetails SubscriptionDetails);
}
