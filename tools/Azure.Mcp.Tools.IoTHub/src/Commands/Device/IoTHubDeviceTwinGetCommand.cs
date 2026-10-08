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
    Id = "a26de064-ca22-4923-8359-396b90e9f856",
    Name = "get",
    Title = "Get IoT Hub Device Twin",
    Description = """
        Get a device twin from an IoT Hub device registry.
        Returns the device twin document including tags and desired/reported properties.
        Device names/IDs are case-sensitive and must match exactly.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubDeviceTwinGetCommand(
    ILogger<IoTHubDeviceTwinGetCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : BaseIoTHubCommand<IoTHubDeviceTwinGetOptions, DeviceTwin>(subscriptionResolver)
{
    private readonly ILogger<IoTHubDeviceTwinGetCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubDeviceTwinGetOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDeviceTwin(
                options.DeviceId,
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.DeviceTwin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device twin '{DeviceId}' from IoT Hub '{HubName}'.", options.DeviceId, options.HubName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
