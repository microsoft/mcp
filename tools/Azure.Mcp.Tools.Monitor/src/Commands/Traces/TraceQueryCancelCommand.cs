// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

[CommandMetadata(
    Id = "1e6c34fd-4b2a-4a1c-9c1f-3a2f1c4d5e6f",
    Name = "cancel",
    Title = "Cancel Distributed Trace Query",
    Description = """
        Cancel (abort, stop, terminate) an in-flight distributed-trace query operation on a
        Trace Store account. Only use this when the user explicitly asks to cancel, stop,
        abort, or terminate a query — do NOT use this to check status or to resume polling
        a still-running query (for that, call `monitor traces query run` again with
        `--operation-id <id>`). Requires the operationId returned by
        `monitor traces query run` when it exited with status=Running.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class TraceQueryCancelCommand(
    ILogger<TraceQueryCancelCommand> logger,
    IMonitorTracesService tracesService,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCommand<TraceQueryCancelOptions, TraceQueryCancelCommand.TraceQueryCancelCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        TraceQueryCancelOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(options.Account))
            {
                throw new ArgumentException("--account is required.", nameof(options));
            }

            if (string.IsNullOrWhiteSpace(options.OperationId))
            {
                throw new ArgumentException("--operation-id is required.", nameof(options));
            }

            var result = await tracesService.CancelQueryAsync(
                options.Subscription!,
                options.Account!,
                options.OperationId!,
                options.Tenant,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                new TraceQueryCancelCommandResult(result),
                MonitorJsonContext.Default.TraceQueryCancelCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling trace query. Subscription: {Subscription}, OperationId: {OperationId}",
                options.Subscription, options.OperationId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TraceQueryCancelCommandResult(TraceQueryCancelResult Result);
}
