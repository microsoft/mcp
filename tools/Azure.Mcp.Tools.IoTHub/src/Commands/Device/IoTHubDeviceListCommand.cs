// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.Device;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
namespace Azure.Mcp.Tools.IoTHub.Commands.Device;

public sealed class IoTHubDeviceListCommand(IIoTHubDeviceService service, ILogger<IoTHubDeviceListCommand> logger)
    : BaseIoTHubCommand<IoTHubDeviceListOptions>
{
    public override string Id => "iothub-device-list";
    public override string Name => "list";
public override string Description => "List devices in an IoT Hub device registry. Returns device identity metadata without authentication keys. " +
    "Use --max-count to limit results (default 100, maximum 100). Values greater than 100 are capped at 100; if more devices exist, truncated=true is set. " +
    "Hub names/IDs are case-sensitive and must match exactly.";
    public override string Title => "List IoT Hub Devices";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    private const int DefaultMaxCount = 100;
    private const int MinMaxCount = 1;
    private const int MaxMaxCount = DefaultMaxCount;

    private readonly IIoTHubDeviceService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubDeviceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.MaxCount.AsOptional());
    }

    protected override IoTHubDeviceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Name.Name);
        var maxCount = parseResult.GetValueOrDefault<int?>(IoTHubOptionDefinitions.MaxCount.Name);
        options.MaxCount = maxCount switch
        {
            null => DefaultMaxCount,
            > MaxMaxCount => MaxMaxCount,
            _ => maxCount.Value
        };
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        if (options.MaxCount < MinMaxCount)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = $"The entered max-count '{options.MaxCount}' is less than 1 device. Please specify a value of at least 1.";
            return context.Response;
        }

        try
        {
            var result = await _service.ListDevices(
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.MaxCount,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.DeviceListResult);

            if (result.Truncated)
            {
                context.Response.Message = $"Showing the first {options.MaxCount} devices. The hub contains more devices, but the results were truncated because the maximum is {options.MaxCount}.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing devices in IoT Hub");
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };
}
