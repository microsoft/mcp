// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Device;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Device;

[CommandMetadata(
    Id = "6ac20d0c-3ff6-486c-b94b-64461ebe891d",
    Name = "stats",
    Title = "Get IoT Hub Device Statistics",
    Description = """
        Get device statistics for an IoT Hub identity registry.
        Returns aggregate device counts for the hub: disabledDeviceCount (the number of currently disabled devices), enabledDeviceCount (the number of currently enabled devices), and totalDeviceCount (the total number of devices registered for the IoT Hub).
        Hub names/IDs are case-sensitive and must match exactly.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubDeviceStatisticsCommand(
    ILogger<IoTHubDeviceStatisticsCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : BaseIoTHubCommand<IoTHubDeviceStatisticsOptions, IoTHubRegistryStatistics>(subscriptionResolver)
{
    private readonly ILogger<IoTHubDeviceStatisticsCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubDeviceStatisticsOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDeviceStatistics(
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.IoTHubRegistryStatistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device statistics for IoT Hub '{HubName}'.", options.HubName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
