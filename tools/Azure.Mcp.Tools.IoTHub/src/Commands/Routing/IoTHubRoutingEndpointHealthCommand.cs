// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Routing;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Routing;

[CommandMetadata(
    Id = "3f2b9a4e-7c1d-4e8a-9b6f-2a4d8c1e5f73",
    Name = "endpoint-health",
    Title = "Get IoT Hub Routing Endpoint Health",
    Description = """
        Get the current IoT Hub-reported health snapshot for one or all message-routing custom endpoints.
        Returns the routingEndpointsHealth fields endpointId, endpointName, healthStatus, lastKnownError,
        lastKnownErrorTime, lastSuccessfulSendAttemptTime, and lastSendAttemptTime without applying a time
        window, querying metrics, or inferring health. This is not a whole-hub health assessment.
        Requires hub-name and resource-group.
        """,
    OperationPlane = ToolOperationPlane.Control,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubRoutingEndpointHealthCommand(
    ILogger<IoTHubRoutingEndpointHealthCommand> logger,
    IIoTHubRoutingService service,
    ISubscriptionResolver subscriptionResolver)
    : BaseIoTHubCommand<IoTHubRoutingEndpointHealthOptions, RoutingEndpointHealthResult>(subscriptionResolver)
{
    private readonly ILogger<IoTHubRoutingEndpointHealthCommand> _logger = logger;
    private readonly IIoTHubRoutingService _service = service;

    public override void ValidateOptions(IoTHubRoutingEndpointHealthOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        IoTHubValidation.ValidateHubName(options.HubName, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubRoutingEndpointHealthOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var endpoints = await _service.GetRoutingEndpointHealth(
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.EndpointName,
                options.Tenant,
                cancellationToken)
                ?? [];

            context.Response.Results = ResponseResult.Create(
                new RoutingEndpointHealthResult(endpoints),
                IoTHubJsonContext.Default.RoutingEndpointHealthResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting routing endpoint health for IoT Hub '{HubName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
                options.HubName,
                options.ResourceGroup,
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        KeyNotFoundException => ex.Message,
        RequestFailedException requestFailed when requestFailed.Status == (int)HttpStatusCode.NotFound =>
            "The IoT Hub was not found. Verify the hub name, resource group, and subscription.",
        RequestFailedException requestFailed when requestFailed.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed reading IoT Hub routing endpoint health. Assign Reader on the IoT Hub or a containing scope.",
        RequestFailedException =>
            "IoT Hub routing endpoint health could not be read.",
        _ => base.GetErrorMessage(ex)
    };
}
