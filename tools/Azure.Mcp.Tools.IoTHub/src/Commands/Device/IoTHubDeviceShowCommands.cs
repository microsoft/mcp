// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.Device;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.Device;

public sealed class IoTHubDeviceShowCommand(IIoTHubDeviceService service, ILogger<IoTHubDeviceShowCommand> logger)
    : BaseIoTHubCommand<IoTHubDeviceShowOptions>
{
    public override string Id => "iothub-device-show";
    public override string Name => "show";
    public override string Description => "Show the device identity for a device in an IoT Hub device registry. Returns the device identity metadata without authentication keys." +
    "Always present the complete raw JSON result to the user, including all fields (statusReason, connectionStateUpdatedTime, statusUpdatedTime,capabilities, etc.). Do not summarize, reformat, or omit any fields." +
    "Device names/IDs are case-sensitive and must match exactly.";
    public override string Title => "Show IoT Hub Device";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    private readonly IIoTHubDeviceService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubDeviceShowCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.DeviceId.AsRequired());
    }

    protected override IoTHubDeviceShowOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Name.Name);
        options.DeviceId = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.DeviceId.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var result = await _service.GetDevice(
                options.DeviceId!,
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.DeviceIdentity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing device in IoT Hub");
            HandleException(context, ex);
        }
        return context.Response;
    }
}
