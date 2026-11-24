// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models.Traces;

namespace Azure.Mcp.Tools.Monitor.Services.Traces;

internal sealed class TraceQueryException(TraceQueryResult partialResult, Exception cause)
    : HttpRequestException(
        "Trace results retrieval failed. Included rows, if any, are incomplete; the next page position may be uncertain. Restart the query.",
        cause,
        cause switch
        {
            HttpRequestException request => request.StatusCode ?? HttpStatusCode.ServiceUnavailable,
            RequestFailedException request => (HttpStatusCode)request.Status,
            JsonException => HttpStatusCode.BadGateway,
            TimeoutException => HttpStatusCode.GatewayTimeout,
            _ => HttpStatusCode.ServiceUnavailable
        })
{
    public TraceQueryResult PartialResult { get; } = partialResult;
}
