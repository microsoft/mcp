// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.IoTHub;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.IoTHub;

public sealed class IoTHubUsageShowCommand(IIoTHubService service, ILogger<IoTHubUsageShowCommand> logger)
    : BaseIoTHubCommand<IoTHubUsageShowOptions>
{
    public override string Id => "iothub-hub-usage-show";
    public override string Name => "usage-show";
    public override string Description =>
        "Show an IoT Hub usage/operational-state snapshot in a single call: connected device count, total device count, " +
        "daily messages used, device-to-cloud (D2C) telemetry messages sent, and throttling errors. " +
        "Connected and total device counts are reported over the requested window as snapshot (latest point-in-time value), " +
        "peak (maximum), and average. Daily messages used resets at 00:00 UTC: a single-day window returns a scalar dailyMessageQuotaUsed, " +
        "while a multi-day window returns a per-day breakdown (dailyMessageQuotaUsedByDay) plus totalMessagesUsed. " +
        "The response also reports the worst single-hour D2C throttling count (peakHourlyThrottlingErrors) alongside the hub's " +
        "sku and units; when any hour exceeds 1000 throttling errors it includes a recommendedSku pointing to the next tier " +
        "(F1->S1, S1->S2, S2->S3) so the hub can be scaled up (no recommendation is made for S3, the top tier). " +
        "Whenever recommendedSku is present, always tell the user their hub is being throttled and recommend scaling it up to that SKU tier to relieve the throttling. " +
        "Use --start-time and --end-time to set the window (ISO 8601); defaults to the last 24 hours.";
    public override string Title => "Show IoT Hub Usage";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    private readonly IIoTHubService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubUsageShowCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.StartTime);
        command.Options.Add(IoTHubOptionDefinitions.EndTime);
    }

    protected override IoTHubUsageShowOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Name.Name);
        options.StartTime = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.StartTime.Name);
        options.EndTime = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.EndTime.Name);
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
            var result = await _service.GetUsageSnapshot(
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.StartTime,
                options.EndTime,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.IoTHubUsageSnapshot);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting IoT Hub usage snapshot");
            HandleException(context, ex);
        }

        return context.Response;
    }
}
