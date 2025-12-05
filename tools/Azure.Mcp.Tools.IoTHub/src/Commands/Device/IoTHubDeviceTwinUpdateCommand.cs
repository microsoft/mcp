// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.Device;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.Device;

public sealed class IoTHubDeviceTwinUpdateCommand(IIoTHubDeviceService service, ILogger<IoTHubDeviceTwinUpdateCommand> logger)
    : SubscriptionCommand<IoTHubDeviceTwinUpdateOptions>
{
    public override string Id => "iothub-device-twin-update";
    public override string Name => "update-twin";
    public override string Description => "Update a device twin in an IoT Hub device registry using a JSON patch document.";
    public override string Title => "Update IoT Hub Device Twin";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = false, OpenWorld = false, ReadOnly = false, LocalRequired = false, Secret = true };

    private readonly IIoTHubDeviceService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubDeviceTwinUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.DeviceId.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Patch.AsRequired());
    }

    protected override IoTHubDeviceTwinUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Name = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Name.Name);
        options.DeviceId = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.DeviceId.Name);
        options.Patch = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Patch.Name);
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
            // Parse the patch JSON
            TwinPatch? patch;
            try
            {
                patch = JsonSerializer.Deserialize(options.Patch!, IoTHubJsonContext.Default.TwinPatch);
                if (patch == null)
                {
                    throw new InvalidOperationException("Failed to parse patch document.");
                }
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid JSON patch document: {ex.Message}", ex);
            }

            var result = await _service.UpdateDeviceTwin(
                options.DeviceId!,
                patch,
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.DeviceTwin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device twin in IoT Hub");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
