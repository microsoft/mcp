// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Search;

public sealed record SearchAggregation
{
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("count")]
    public long Count { get; init; }
}
