// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Adme.Models.Schema;

/// <summary>
/// Describes an ADME schema and its publication metadata.
/// </summary>
public sealed record SchemaInfo
{
    [JsonPropertyName("schemaIdentity")]
    public SchemaIdentity? SchemaIdentity { get; init; }

    [JsonPropertyName("status")]
    public string? Status { get; init; }

    [JsonPropertyName("scope")]
    public string? Scope { get; init; }

    [JsonPropertyName("dateCreated")]
    public string? DateCreated { get; init; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; init; }
}
