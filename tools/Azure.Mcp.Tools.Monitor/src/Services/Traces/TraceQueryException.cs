// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Monitor.Models.Traces;

namespace Azure.Mcp.Tools.Monitor.Services.Traces;

internal sealed class TraceQueryException(TraceQueryResult partialResult, Exception cause)
    : HttpRequestException(
        "Trace results retrieval failed after receiving data. Included rows are incomplete; the next page position may be uncertain.",
        cause,
        cause switch
        {
            HttpRequestException request => request.StatusCode ?? HttpStatusCode.ServiceUnavailable,
            RequestFailedException request => (HttpStatusCode)request.Status,
            JsonException => HttpStatusCode.BadGateway,
            _ => HttpStatusCode.ServiceUnavailable
        })
{
    public TraceQueryResult PartialResult { get; } = partialResult;
}
