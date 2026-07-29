// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Device;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Device;

[CommandMetadata(
    Id = "63c331ab-3848-4d2b-86b2-4d17d187d5cf",
    Name = "show",
    Title = "Show IoT Hub Device",
    Description = """
        Show the device identity for a device in an IoT Hub device registry. Returns the device identity metadata without authentication keys.
        Always present the complete raw JSON result to the user, including all fields (statusReason, connectionStateUpdatedTime, statusUpdatedTime, capabilities, etc.). Do not summarize, reformat, or omit any fields.
        Device names/IDs are case-sensitive and must match exactly.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubDeviceShowCommand(
    ILogger<IoTHubDeviceShowCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IoTHubDeviceShowOptions, DeviceIdentity>(subscriptionResolver)
{
    private readonly ILogger<IoTHubDeviceShowCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubDeviceShowOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetDevice(
                options.DeviceId,
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.DeviceIdentity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing device '{DeviceId}' in IoT Hub '{HubName}'.", options.DeviceId, options.HubName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
