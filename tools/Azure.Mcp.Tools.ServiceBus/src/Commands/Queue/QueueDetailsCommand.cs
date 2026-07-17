// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.ServiceBus.Models;
using Azure.Mcp.Tools.ServiceBus.Options.Queue;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ServiceBus.Commands.Queue;

[CommandMetadata(
    Id = "a02c58ce-e89f-4303-ac4a-c9dfb118e761",
    Name = "details",
    Title = "Get Service Bus Queue Details",
    Description = """
        Get details about a Service Bus queue. Returns queue properties and runtime information. Properties returned include
        lock duration, max message size, queue size, creation date, status, current message counts, etc.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class QueueDetailsCommand(ILogger<QueueDetailsCommand> logger, IServiceBusService serviceBusService)
    : AuthenticatedCommand<BaseQueueOptions, QueueDetailsCommand.QueueDetailsCommandResult>
{
    private readonly ILogger<QueueDetailsCommand> _logger = logger;
    private readonly IServiceBusService _serviceBusService = serviceBusService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseQueueOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var details = await _serviceBusService.GetQueueDetails(
                options.Namespace,
                options.Queue,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

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

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ServiceBusException sbEx when sbEx.Reason == ServiceBusFailureReason.MessagingEntityNotFound => HttpStatusCode.NotFound,
        _ => base.GetStatusCode(ex)
    };

    public sealed record QueueDetailsCommandResult(QueueDetails QueueDetails);
}
