// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// Result of cancelling a running trace-query operation.
/// </summary>
public sealed class TraceQueryCancelResult
{
    public required string OperationId { get; init; }

    public required string Status { get; init; }
}
