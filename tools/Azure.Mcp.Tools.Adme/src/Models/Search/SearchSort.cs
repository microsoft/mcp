// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Search;

public sealed record SearchSort
{
    [JsonPropertyName("field")]
    public required IReadOnlyList<string> Field { get; init; }

    [JsonPropertyName("order")]
    public required IReadOnlyList<string> Order { get; init; }

    [JsonPropertyName("filter")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Filter { get; init; }
}
