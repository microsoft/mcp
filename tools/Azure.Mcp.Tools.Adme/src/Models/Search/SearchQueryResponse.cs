// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Search;

public sealed record SearchQueryResponse
{
    [JsonPropertyName("results")]
    public required IReadOnlyList<JsonElement> Results { get; init; }

    [JsonPropertyName("aggregations")]
    public IReadOnlyList<SearchAggregation>? Aggregations { get; init; }

    [JsonPropertyName("phraseSuggestions")]
    public IReadOnlyList<string>? PhraseSuggestions { get; init; }

    [JsonPropertyName("totalCount")]
    public long? TotalCount { get; init; }
}