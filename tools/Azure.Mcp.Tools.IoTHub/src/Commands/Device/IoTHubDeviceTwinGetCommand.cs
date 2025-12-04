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

public sealed class IoTHubDeviceTwinGetCommand(IIoTHubDeviceService service, ILogger<IoTHubDeviceTwinGetCommand> logger)
    : SubscriptionCommand<IoTHubDeviceTwinGetOptions>
{
    public override string Id => "iothub-device-twin-get";
    public override string Name => "get-twin";
    public override string Description => "Get a device twin from an IoT Hub device registry.";
    public override string Title => "Get IoT Hub Device Twin";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = true };

    private readonly IIoTHubDeviceService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubDeviceTwinGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.DeviceId.AsRequired());
    }

    protected override IoTHubDeviceTwinGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var result = await _service.GetDeviceTwin(
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.DeviceId!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.DeviceTwin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device twin from IoT Hub");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
