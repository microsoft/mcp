// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Search;

public sealed record SearchCursorResponse
{
    [JsonPropertyName("results")]
    public required IReadOnlyList<JsonElement> Results { get; init; }

    [JsonPropertyName("cursor")]
    public string? Cursor { get; init; }

    [JsonPropertyName("totalCount")]
    public long? TotalCount { get; init; }
}