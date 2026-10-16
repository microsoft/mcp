// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.Traces;

/// <summary>
/// Shared error mapping for the <c>monitor traces</c> commands. Failures arrive as
/// <see cref="HttpRequestException"/> carrying the backend status; translate the actionable ones
/// rather than echoing raw response bodies.
/// </summary>
public abstract class BaseTraceCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption
{
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        TraceQueryException queryError => queryError.Message,
        HttpRequestException { StatusCode: HttpStatusCode.Forbidden } =>
            "Authorization failed on the trace account. The caller needs read access on the Azure Monitor Workspace.",
        HttpRequestException { StatusCode: HttpStatusCode.NotFound } =>
            "Workspace or operation not found. Verify --resource-group and --workspace with 'monitor traces account list'.",
        HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests } =>
            "Trace Store throttled the request. Narrow --start-time/--end-time, add filters, or lower --max-results.",
        HttpRequestException { StatusCode: HttpStatusCode.BadRequest } badRequest => badRequest.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override void HandleException(CommandContext context, Exception ex)
    {
        base.HandleException(context, ex);
        if (ex is TraceQueryException queryError)
        {
            context.Response.Results = ResponseResult.Create(
                new TraceQueryCommandResult(queryError.PartialResult),
                MonitorJsonContext.Default.TraceQueryCommandResult);
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        // Sovereign clouds have no published trace data-plane audience yet.
        NotSupportedException => HttpStatusCode.NotImplemented,
        _ => base.GetStatusCode(ex)
    };
}
