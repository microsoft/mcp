// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// Wire body for the trace data-plane submit endpoint
/// (POST {amwEndpoint}/traces:query?api-version=...).
/// </summary>
internal sealed class TraceQueryRequest
{
    public required string TraceAccountId { get; init; }

    public required string Query { get; init; }

    public required string Language { get; init; }

    public required TraceQueryTimeRange TimeRange { get; init; }

    public required TraceQueryOptionsBody Options { get; init; }
}
