// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents a cursor-paginated page of record ids for one kind.
/// </summary>
public sealed record QueryRecordsResponse
{
    [JsonPropertyName("cursor")]
    public string? Cursor { get; init; }

    [JsonPropertyName("results")]
    public required IReadOnlyList<string> Results { get; init; }
}
