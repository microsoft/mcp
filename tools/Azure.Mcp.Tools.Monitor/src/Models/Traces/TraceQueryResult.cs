// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// Collected query rows and retrieval status. Resume when a time or row budget stops retrieval.
/// </summary>
public sealed class TraceQueryResult
{
    public required string OperationId { get; init; }

    public required string Status { get; init; }

    public JsonElement? Payload { get; init; }

    public bool OutputLimitReached { get; init; }
}
