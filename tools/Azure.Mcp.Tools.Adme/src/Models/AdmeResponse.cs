// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models;

public sealed class AdmeResponse<T>(T result, string? correlationId)
{
    public T Result { get; } = result;

    public string? CorrelationId { get; } = correlationId;

    [JsonPropertyName("nextCursor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NextCursor { get; init; }
}
