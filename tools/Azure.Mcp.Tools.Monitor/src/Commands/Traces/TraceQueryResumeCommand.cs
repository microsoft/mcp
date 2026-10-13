// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Options.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

[CommandMetadata(
    Id = "2c18b005-209c-4f41-9c60-9c6a3cc80a17",
    Name = "resume",
    Title = "Resume Distributed Trace Query",
    Description = """
        Resume polling an existing distributed-trace (Trace Store) query by operation id,
        without submitting a new query. Requires the Azure Monitor Workspace and resource group.
        Polls up to 180 seconds; returns Succeeded with the first results page, or Running
        with the same operationId. Use `monitor traces query run` for a new query, or
        `monitor traces query cancel` to stop one.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TraceQueryResumeCommand(
    ILogger<TraceQueryResumeCommand> logger,
    IMonitorTracesService tracesService,
    ISubscriptionResolver subscriptionResolver)
    : BaseTraceCommand<TraceQueryResumeOptions, TraceQueryResumeCommand.TraceQueryResumeCommandResult>(subscriptionResolver)
{
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        TraceQueryResumeOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await tracesService.ResumeQueryAsync(
                options.Subscription!, options.ResourceGroup, options.Workspace, options.OperationId,
                Math.Clamp(options.PageSize ?? 100, 1, 1000), options.IncludeSpans ?? true,
                options.Tenant, cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new TraceQueryResumeCommandResult(result),
                MonitorJsonContext.Default.TraceQueryResumeCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resuming trace query. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Workspace: {Workspace}",
                options.Subscription, options.ResourceGroup, options.Workspace);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TraceQueryResumeCommandResult(TraceQueryResult Result);
}
