// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.Traces;

/// <summary>
/// Wire body for trace query and analysis submissions.
/// </summary>
internal sealed class TraceQueryRequest
{
    public required string TraceAccountId { get; init; }

    public required string Query { get; init; }

    public required string Language { get; init; }

    public required TraceQueryTimeRange TimeRange { get; init; }

    public required TraceQueryOptionsBody Options { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; init; }
}
