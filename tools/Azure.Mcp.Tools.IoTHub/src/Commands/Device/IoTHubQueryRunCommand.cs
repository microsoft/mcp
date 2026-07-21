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
using Azure.Mcp.Tools.IoTHub.Models;
using Azure.Mcp.Tools.IoTHub.Options;
using Azure.Mcp.Tools.IoTHub.Options.IoTHub;
using Azure.Mcp.Tools.IoTHub.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.IoTHub.Commands.IoTHub;

public sealed class IoTHubQueryRunCommand(IIoTHubDeviceService service, ILogger<IoTHubQueryRunCommand> logger)
    : BaseIoTHubCommand<IoTHubQueryRunOptions>
{
    public override string Id => "iothub-query-run";
    public override string Name => "run";
    public override string Description => "Run an IoT Hub SQL-like query against an IoT Hub. Supports read/search queries over device identities, modules, and jobs (e.g. 'SELECT * FROM devices', 'SELECT * FROM devices.modules', 'SELECT * FROM devices.jobs'). Returns exactly one page of matching records as raw JSON. For natural-language list/show requests, especially filtered device requests, use a compact projection such as 'SELECT deviceId, status, connectionState FROM devices WHERE ...' and add only the requested tag or property fields; avoid 'SELECT *' unless the user explicitly asks for raw device twins, all fields, modules, jobs, or full JSON. Use --max-count to set the page size (default 100, maximum 100). Values greater than 100 are capped at 100, so one page is always at most 100 items. Never make repeated calls or loop for additional pages in a single user request, no matter whether the user asks for all devices, the rest of the devices, every device, more than 100 matching records, or any large number. Return exactly one page and, when hasMore is true, include the continuationToken for a later explicit next-page request. The --continuation-token input must be the opaque continuationToken string returned by a previous iothub_query_run response; do not pass hasMore=true/false or any boolean value. To simply list devices through MCP - including a small, specific number such as 'show me 2 devices' - prefer the 'iothub device list' command instead; use this query command for SQL-like filtering, modules, jobs, large device sets, or paging through more than 100 records.";
    public override string Title => "Run IoT Hub Query";
    public override ToolMetadata Metadata => new() { Destructive = false, Idempotent = true, OpenWorld = false, ReadOnly = true, LocalRequired = false, Secret = false };

    private const int DefaultMaxCount = 100;
    private const int MinMaxCount = 1;
    private const int MaxMaxCount = DefaultMaxCount;

    private readonly IIoTHubDeviceService _service = service ?? throw new ArgumentNullException(nameof(service));
    private readonly ILogger<IoTHubQueryRunCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(IoTHubOptionDefinitions.Name.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.Query.AsRequired());
        command.Options.Add(IoTHubOptionDefinitions.QueryMaxCount.AsOptional());
        command.Options.Add(IoTHubOptionDefinitions.ContinuationToken.AsOptional());
    }

    protected override IoTHubQueryRunOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Name = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Name.Name);
        options.Query = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.Query.Name);
        var maxCount = parseResult.GetValueOrDefault<int?>(IoTHubOptionDefinitions.MaxCount.Name);
        options.MaxCount = maxCount switch
        {
            null => DefaultMaxCount,
            > MaxMaxCount => MaxMaxCount,
            _ => maxCount.Value
        };
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(IoTHubOptionDefinitions.ContinuationToken.Name);
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

        if (IsBooleanContinuationToken(options.ContinuationToken))
        {
            context.Response.Status = HttpStatusCode.BadRequest;
            context.Response.Message = "The continuation-token value must be the opaque continuationToken string returned by a previous iothub_query_run response, not hasMore=true/false. Omit --continuation-token to fetch the first page.";
            return context.Response;
        }

        try
        {
            _logger.LogInformation(
                "Starting IoT Hub query. MaxCount={MaxCount}, HasContinuationToken={HasContinuationToken}",
                options.MaxCount,
                !string.IsNullOrEmpty(options.ContinuationToken));

            var stopwatch = Stopwatch.StartNew();
            var page = await _service.RunQuery(
                options.Query!,
                options.Name!,
                options.ResourceGroup!,
                options.Subscription!,
                options.MaxCount,
                options.ContinuationToken,
                options.RetryPolicy,
                cancellationToken);
            stopwatch.Stop();

            _logger.LogInformation(
                "IoT Hub query completed. MaxCount={MaxCount}, Returned={Returned}, HasContinuationToken={HasContinuationToken}, ElapsedMs={ElapsedMs}",
                options.MaxCount,
                page.Items.Count,
                !string.IsNullOrEmpty(page.ContinuationToken),
                stopwatch.ElapsedMilliseconds);

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

            context.Response.Results = ResponseResult.Create(result, IoTHubJsonContext.Default.IoTHubQueryRunResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running query against IoT Hub");
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
