// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Routing;
using Azure.Mcp.Tools.IoTHub.Routing;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Routing;

[CommandMetadata(
    Id = "c2a9f4e1-7b6d-4f80-9a35-1e8c6d2b4f79",
    Name = "endpoint-diagnostics",
    Title = "Get IoT Hub Routing Endpoint Diagnostics",
    Description = """
        Get factual time-windowed diagnostic evidence for one or all IoT Hub message-routing custom
        endpoints. Returns per-endpoint IoT Hub RoutingDeliveries and RoutingDeliveryLatency buckets,
        current target existence, and native target-resource Azure Monitor metrics for Event Hubs,
        Service Bus, Blob Storage, and Cosmos DB. The tool does not infer health, confidence, or likely
        cause or assess whole-hub health. No returned values do not mean zero activity. Target metrics
        describe the shared parent resource and cannot be attributed to this endpoint.
        Use paired --start-time and --end-time for an absolute UTC range; if omitted, the previous
        24 hours are used. --interval defaults to PT1H. At most 720 buckets are allowed.
        Requires hub-name and resource-group.
        """,
    OperationPlane = ToolOperationPlane.Both,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubRoutingEndpointDiagnosticsCommand(
    ILogger<IoTHubRoutingEndpointDiagnosticsCommand> logger,
    IIoTHubRoutingService service,
    ISubscriptionResolver subscriptionResolver)
    : BaseIoTHubCommand<IoTHubRoutingEndpointDiagnosticsOptions, RoutingEndpointDiagnostics>(subscriptionResolver)
{
    private readonly ILogger<IoTHubRoutingEndpointDiagnosticsCommand> _logger = logger;
    private readonly IIoTHubRoutingService _service = service;

    public override void ValidateOptions(
        IoTHubRoutingEndpointDiagnosticsOptions options,
        ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        IoTHubValidation.ValidateHubName(options.HubName, validationResult);
        RoutingDiagnosticsWindow.Validate(
            options.StartTime,
            options.EndTime,
            options.Interval,
            validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubRoutingEndpointDiagnosticsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetRoutingEndpointDiagnostics(
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.EndpointName,
                options.StartTime,
                options.EndTime,
                options.Interval,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.RoutingEndpointDiagnostics);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting routing endpoint diagnostics for IoT Hub '{HubName}' in resource group '{ResourceGroup}' and subscription '{Subscription}'.",
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
            "Authorization failed reading IoT Hub routing diagnostics. Assign Reader on the IoT Hub or a containing scope.",
        RequestFailedException =>
            "IoT Hub routing diagnostics could not be read.",
        _ => base.GetErrorMessage(ex)
    };
}
