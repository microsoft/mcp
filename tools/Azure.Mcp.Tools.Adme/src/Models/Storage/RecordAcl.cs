// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents the access-control list carried on every OSDU record.
/// </summary>
public sealed record RecordAcl
{
    [JsonPropertyName("viewers")]
    public IReadOnlyList<string>? Viewers { get; init; }

    [JsonPropertyName("owners")]
    public IReadOnlyList<string>? Owners { get; init; }
}
