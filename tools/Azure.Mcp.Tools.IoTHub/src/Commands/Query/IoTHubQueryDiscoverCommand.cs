// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Query;
using Azure.Mcp.Tools.IoTHub.Query;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Query;

[CommandMetadata(
    Id = "d1fed1c3-1e26-44c2-82a6-bd944f85893d",
    Name = "discover",
    Title = "Discover IoT Hub Query Fields",
    Description = """
        Discover queryable IoT Hub device twin field paths by sampling a small page of devices with 'SELECT * FROM devices'.
        Use this before 'iothub query compile' when the user asks for fields whose exact twin path is unknown. The command returns compact field paths grouped by scope: device, tags, desired, and reported, with observed JSON types and example values.
        Pass the returned 'fields' object to 'iothub query compile --discovered-fields' so compile can reject nonexistent fields and suggest matching nested paths. Defaults to sampling 10 devices; --max-count controls the sample size and is capped at 100.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubQueryDiscoverCommand(
    ILogger<IoTHubQueryDiscoverCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IoTHubQueryDiscoverOptions, IoTHubQueryDiscoverResult>(subscriptionResolver)
{
    private const string DiscoverQuery = "SELECT * FROM devices";
    private const int DefaultMaxCount = 10;
    private const int MinMaxCount = 1;
    private const int MaxMaxCount = 100;

    private readonly ILogger<IoTHubQueryDiscoverCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubQueryDiscoverOptions options,
        CancellationToken cancellationToken)
    {
        var maxCount = options.MaxCount switch
        {
            null => DefaultMaxCount,
            > MaxMaxCount => MaxMaxCount,
            _ => options.MaxCount.Value
        };

        if (maxCount < MinMaxCount)
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = $"The entered max-count '{maxCount}' is less than 1 item. Please specify a value of at least {MinMaxCount}.";
            return context.Response;
        }

        try
        {
            var page = await _service.RunQuery(
                DiscoverQuery,
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                maxCount,
                null,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var fields = IoTHubQueryFieldDiscoverer.Discover(page.Items);
            var totalFieldCount = fields.Device.Count + fields.Tags.Count + fields.Desired.Count + fields.Reported.Count;
            var message = $"Discovered {totalFieldCount} queryable field paths from {page.Items.Count} sampled device twins. Use the fields object with iothub query compile --discovered-fields for strict field validation.";
            var result = new IoTHubQueryDiscoverResult(fields, page.Items.Count, maxCount, message);

            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.IoTHubQueryDiscoverResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering IoT Hub query fields for '{HubName}'.", options.HubName);
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
