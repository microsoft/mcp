// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents the request body of a multi-record fetch.
/// </summary>
public sealed record FetchRecordsRequest
{
    [JsonPropertyName("records")]
    public required IReadOnlyList<string> Records { get; init; }

    [JsonPropertyName("attributes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Attributes { get; init; }
}
