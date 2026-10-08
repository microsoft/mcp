// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents the system-level lineage of an OSDU record.
/// </summary>
public sealed record RecordAncestry
{
    /// <summary>
    /// Gets the versioned parent record ids, which always carry a trailing version segment.
    /// </summary>
    [JsonPropertyName("parents")]
    public IReadOnlyList<string>? Parents { get; init; }
}
