// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Diagnostics;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Commands;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.IoTHub;
using Azure.Mcp.Tools.IoTHub.Query;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.IoTHub;

public sealed class IoTHubQueryDiscoverCommand(IIoTHubDeviceService service, ILogger<IoTHubQueryDiscoverCommand> logger)
    : SubscriptionCommand<IoTHubQueryDiscoverOptions>
{
    public override string Id => "iothub-query-discover";
    public override string Name => "discover";
    public override string Description => "Discover queryable IoT Hub device twin field paths by sampling a small page of devices with 'SELECT * FROM devices'. " +
        "Use this before 'iothub query compile' when the user asks for fields whose exact twin path is unknown. The command returns compact field paths grouped by scope: " +
        "device, tags, desired, and reported, with observed JSON types and example values. Pass the returned 'fields' object to 'iothub query compile --discovered-fields' " +
        "so compile can reject nonexistent fields and suggest matching nested paths. Defaults to sampling 10 devices; --max-count controls the sample size and is capped at 100.";
    public override string Title => "Discover IoT Hub Query Fields";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    private const string DiscoverQuery = "SELECT * FROM devices";
    private const int DefaultMaxCount = 10;
    private const int MinMaxCount = 1;
    private const int MaxMaxCount = 100;

    private readonly IIoTHubDeviceService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubQueryDiscoverCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.QueryMaxCount.AsOptional());
    }

    protected override IoTHubQueryDiscoverOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            context.Response.Message = $"The entered max-count '{options.MaxCount}' is less than 1 item. Please specify a value of at least {MinMaxCount}.";
            return context.Response;
        }

        try
        {
            _logger.LogInformation("Starting IoT Hub query field discovery. MaxCount={MaxCount}", options.MaxCount);

            var stopwatch = Stopwatch.StartNew();
            var page = await _service.RunQuery(
                DiscoverQuery,
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.MaxCount,
                null,
                options.RetryPolicy,
                cancellationToken);
            stopwatch.Stop();

            var fields = IoTHubQueryFieldDiscoverer.Discover(page.Items);
            var totalFieldCount = fields.Device.Count + fields.Tags.Count + fields.Desired.Count + fields.Reported.Count;
            var message = $"Discovered {totalFieldCount} queryable field paths from {page.Items.Count} sampled device twins. Use the fields object with iothub query compile --discovered-fields for strict field validation.";
            var result = new IoTHubQueryDiscoverResult(fields, page.Items.Count, options.MaxCount!.Value, message);

            _logger.LogInformation(
                "IoT Hub query field discovery completed. MaxCount={MaxCount}, SampleCount={SampleCount}, FieldCount={FieldCount}, ElapsedMs={ElapsedMs}",
                options.MaxCount,
                page.Items.Count,
                totalFieldCount,
                stopwatch.ElapsedMilliseconds);

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.IoTHubQueryDiscoverResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering IoT Hub query fields");
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
