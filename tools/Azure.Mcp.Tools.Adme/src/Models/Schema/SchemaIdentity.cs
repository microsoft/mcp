// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Schema;

/// <summary>
/// Identifies an ADME schema kind and version.
/// </summary>
public sealed record SchemaIdentity
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("authority")]
    public string? Authority { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("entityType")]
    public string? EntityType { get; init; }

    [JsonPropertyName("schemaVersionMajor")]
    public int? SchemaVersionMajor { get; init; }

    [JsonPropertyName("schemaVersionMinor")]
    public int? SchemaVersionMinor { get; init; }

    [JsonPropertyName("schemaVersionPatch")]
    public int? SchemaVersionPatch { get; init; }
}
