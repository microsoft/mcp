// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents the version history of a single OSDU record.
/// </summary>
public sealed record RecordVersionsResponse
{
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>
    /// Gets the numeric version identifiers, ordered from oldest to newest.
    /// </summary>
    [JsonPropertyName("versions")]
    public required IReadOnlyList<long> Versions { get; init; }
}
