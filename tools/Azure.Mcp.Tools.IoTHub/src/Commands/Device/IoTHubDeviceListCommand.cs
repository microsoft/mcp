// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
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
    Id = "426028ac-ac9b-4348-b549-74c0b53ad3e7",
    Name = "list",
    Title = "List IoT Hub Devices",
    Description = """
        List devices in an IoT Hub device registry. Returns device identity metadata without authentication keys.
        Use --max-count to limit results (default 100, maximum 100). Values greater than 100 are rejected with an error; if more devices exist, truncated=true is set.
        Hub names/IDs are case-sensitive and must match exactly.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubDeviceListCommand(
    ILogger<IoTHubDeviceListCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IoTHubDeviceListOptions, DeviceListResult>(subscriptionResolver)
{
    private const int DefaultMaxCount = 100;
    private const int MinMaxCount = 1;
    private const int MaxMaxCount = DefaultMaxCount;

    private readonly ILogger<IoTHubDeviceListCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override void ValidateOptions(IoTHubDeviceListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.MaxCount is < MinMaxCount)
        {
            validationResult.Errors.Add(
                $"The entered max-count '{options.MaxCount}' is less than 1 device. Please specify a value of at least 1.");
        }

        if (options.MaxCount is > MaxMaxCount)
        {
            validationResult.Errors.Add(
                $"The entered max-count '{options.MaxCount}' is greater than the maximum of {MaxMaxCount} devices. Please specify a value of at most {MaxMaxCount}.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubDeviceListOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var maxCount = options.MaxCount ?? DefaultMaxCount;

            var result = await _service.ListDevices(
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                options.Tenant,
                maxCount,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.DeviceListResult);

            if (result.Truncated)
            {
                context.Response.Message =
                    $"Showing the first {maxCount} devices. The hub may contain more devices; the results were truncated at the maximum of {maxCount}.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing devices in IoT Hub '{HubName}'.", options.HubName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
