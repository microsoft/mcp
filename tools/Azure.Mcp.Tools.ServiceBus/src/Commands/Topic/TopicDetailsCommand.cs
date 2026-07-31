// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Options.Topic;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ServiceBus.Commands.Topic;

[CommandMetadata(
    Id = "c2487c40-58d0-40f7-98f1-105744865a11",
    Name = "details",
    Title = "Get Service Bus Topic Details",
    Description = """
        Retrieves details about a Service Bus topic. Returns runtime information and topic properties including number of subscriptions, max message size, max topic size, number of scheduled messages, etc.
        Required arguments are namespace: The fully qualified Service Bus namespace host name (usually in the form <namespace>.servicebus.windows.net) and topic: Topic name to get information about.
        Do not use this to get details on Service Bus subscription- instead use servicebus_topic_subscription_details.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TopicDetailsCommand(ILogger<TopicDetailsCommand> logger, IServiceBusService serviceBusService)
    : AuthenticatedCommand<BaseTopicOptions, TopicDetailsCommand.TopicDetailsCommandResult>
{
    private readonly ILogger<TopicDetailsCommand> _logger = logger;
    private readonly IServiceBusService _serviceBusService = serviceBusService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseTopicOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var details = await _serviceBusService.GetTopicDetails(
                options.Namespace,
                options.Topic,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(details), ServiceBusJsonContext.Default.TopicDetailsCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Service Bus topic details");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ServiceBusException exception when exception.Reason == ServiceBusFailureReason.MessagingEntityNotFound =>
            $"Topic not found. Please check the topic name and try again.",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ServiceBusException sbEx when sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound => HttpStatusCode.NotFound,
        _ => base.GetStatusCode(ex)
    };

    public sealed record TopicDetailsCommandResult(TopicDetails TopicDetails);
}
