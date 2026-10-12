// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Tools.Adme.Models.Search;

public sealed record SearchResponse
{
    public required IReadOnlyList<JsonElement> Results { get; init; }

    public long? TotalCount { get; init; }

    public string? Cursor { get; init; }

    public IReadOnlyList<SearchAggregation>? Aggregations { get; init; }

    public IReadOnlyList<string>? PhraseSuggestions { get; init; }
}
