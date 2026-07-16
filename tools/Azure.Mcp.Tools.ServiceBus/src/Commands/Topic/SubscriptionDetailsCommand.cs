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
    Id = "578edf30-01f3-45da-b451-3932dcce7cc5",
    Name = "details",
    Title = "Get Service Bus Topic Subscription Details",
    Description = """
        Get details about a Service Bus subscription. Returns subscription runtime properties including message counts, delivery settings, and other metadata.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SubscriptionDetailsCommand(ILogger<SubscriptionDetailsCommand> logger, IServiceBusService serviceBusService)
    : AuthenticatedCommand<SubscriptionDetailsOptions, SubscriptionDetailsCommand.SubscriptionDetailsCommandResult>
{
    private readonly ILogger<SubscriptionDetailsCommand> _logger = logger;
    private readonly IServiceBusService _serviceBusService = serviceBusService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SubscriptionDetailsOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var details = await _serviceBusService.GetSubscriptionDetails(
                options.Namespace,
                options.Topic,
                options.SubscriptionName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(details), ServiceBusJsonContext.Default.SubscriptionDetailsCommandResult);
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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ServiceBusException sbEx when sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound => HttpStatusCode.NotFound,
        _ => base.GetStatusCode(ex)
    };

    public sealed record SubscriptionDetailsCommandResult(SubscriptionDetails SubscriptionDetails);
}
