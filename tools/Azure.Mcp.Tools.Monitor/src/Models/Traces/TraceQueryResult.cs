// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// Result of a distributed-trace query. When <see cref="Status"/> is <c>"Succeeded"</c>,
/// <see cref="Payload"/> contains the raw TQC response body (rows + schema). When
/// <see cref="Status"/> is <c>"Running"</c>, the poll budget was exhausted before the
/// query completed — the caller can resume by passing <see cref="OperationId"/> as
/// <c>--operation-id</c> to <c>monitor traces query run</c>.
/// </summary>
public sealed class TraceQueryResult
{
    public required string OperationId { get; init; }

    public required string Status { get; init; }

    public JsonElement? Payload { get; init; }
}
