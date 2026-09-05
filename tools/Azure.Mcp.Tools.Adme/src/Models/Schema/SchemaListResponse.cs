// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Schema;

/// <summary>
/// Represents a paged list of ADME schema descriptors.
/// </summary>
public sealed record SchemaListResponse
{
    [JsonPropertyName("schemaInfos")]
    public required IReadOnlyList<SchemaInfo> SchemaInfos { get; init; }

    [JsonPropertyName("offset")]
    public int? Offset { get; init; }

    [JsonPropertyName("count")]
    public int? Count { get; init; }

    [JsonPropertyName("totalCount")]
    public int? TotalCount { get; init; }
}
