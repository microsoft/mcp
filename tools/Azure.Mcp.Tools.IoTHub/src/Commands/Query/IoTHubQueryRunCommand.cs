// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options.Query;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.IoTHub.Commands.Query;

[CommandMetadata(
    Id = "2d8e60b2-87e5-4e2d-ac91-615e02e2ac21",
    Name = "run",
    Title = "Run IoT Hub Query",
    Description = """
        Run an IoT Hub SQL-like query against the device registry and return a single page of results.
        Prefer projecting only the specific property fields you need; avoid 'SELECT *' unless the user explicitly asks for raw device twins, all fields, modules, jobs, or full JSON.
        Use --max-count to set the page size (default 100, maximum 100). Values greater than 100 are capped at 100, so one page is always at most 100 items.
        Never make repeated calls or loop for additional pages in a single user request. Return exactly one page and, when hasMore is true, include the continuationToken for a later explicit next-page request.
        The --continuation-token input must be the opaque continuationToken string returned by a previous iothub_query_run response; do not pass hasMore=true/false or any boolean value.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class IoTHubQueryRunCommand(
    ILogger<IoTHubQueryRunCommand> logger,
    IIoTHubDeviceService service,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<IoTHubQueryRunOptions, IoTHubQueryRunResult>(subscriptionResolver)
{
    private const int DefaultMaxCount = 100;
    private const int MinMaxCount = 1;
    private const int MaxMaxCount = DefaultMaxCount;

    private readonly ILogger<IoTHubQueryRunCommand> _logger = logger;
    private readonly IIoTHubDeviceService _service = service;

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        IoTHubQueryRunOptions options,
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

        if (IsBooleanContinuationToken(options.ContinuationToken))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The continuation-token value must be the opaque continuationToken string returned by a previous iothub_query_run response, not hasMore=true/false. Omit --continuation-token to fetch the first page.";
            return context.Response;
        }

        try
        {
            var page = await _service.RunQuery(
                options.Query,
                options.HubName,
                options.ResourceGroup,
                options.Subscription!,
                maxCount,
                options.ContinuationToken,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var hasMore = !string.IsNullOrEmpty(page.ContinuationToken);
            var message = hasMore
                ? $"Showing {page.Items.Count} results. More results are available; return this page now and use the continuationToken only on a later explicit next-page request."
                : $"Showing {page.Items.Count} results. No more results are available.";

            var result = new IoTHubQueryRunResult(
                page.Items,
                page.Items.Count,
                hasMore,
                page.ContinuationToken,
                message);

            context.Response.Results = ResponseResult.Create(
                result,
                IoTHubJsonContext.Default.IoTHubQueryRunResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running query against IoT Hub '{HubName}'.", options.HubName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        TimeoutException => HttpStatusCode.RequestTimeout,
        _ => base.GetStatusCode(ex)
    };

    private static bool IsBooleanContinuationToken(string? continuationToken)
    {
        var normalizedToken = continuationToken?.Trim();
        return string.Equals(normalizedToken, bool.TrueString, StringComparison.OrdinalIgnoreCase)
            || string.Equals(normalizedToken, bool.FalseString, StringComparison.OrdinalIgnoreCase);
    }
}
