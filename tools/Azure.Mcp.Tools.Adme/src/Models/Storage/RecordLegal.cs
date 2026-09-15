// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Storage;

/// <summary>
/// Represents the legal-tag envelope carried on every OSDU record.
/// </summary>
public sealed record RecordLegal
{
    [JsonPropertyName("legaltags")]
    public IReadOnlyList<string>? LegalTags { get; init; }

    [JsonPropertyName("otherRelevantDataCountries")]
    public IReadOnlyList<string>? OtherRelevantDataCountries { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }
}
