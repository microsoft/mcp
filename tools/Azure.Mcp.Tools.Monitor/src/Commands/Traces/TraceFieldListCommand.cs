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
    Id = "27d4b3e0-8b7d-4c3a-a2c5-8f7a5a0f6a1e",
    Name = "list",
    Title = "List Distributed Trace Field Names",
    Description = """
        List the field names exposed by a distributed-trace (Trace Store) account. Returns two
        buckets: `traceFields` (attributes on the trace row, e.g. trace_id, duration_nano,
        span_count) and `spanFields` (attributes on individual spans, e.g. name,
        service.name, http.request.method, and the nested `attributes` struct). Use this to
        discover the schema before composing KQL queries for `monitor traces query run`.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TraceFieldListCommand(
    ILogger<TraceFieldListCommand> logger,
    IMonitorTracesService tracesService,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCommand<TraceFieldListOptions, TraceFieldListCommand.TraceFieldListCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        TraceFieldListOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(options.Account))
            {
                throw new ArgumentException("--account is required.", nameof(options));
            }

            var fields = await tracesService.ListFieldNamesAsync(
                options.Subscription!,
                options.Account!,
                options.Tenant,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                new TraceFieldListCommandResult(fields),
                MonitorJsonContext.Default.TraceFieldListCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error listing trace field names. Subscription: {Subscription}, Account: {Account}",
                options.Subscription, options.Account);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TraceFieldListCommandResult(TraceFieldNames Fields);
}
