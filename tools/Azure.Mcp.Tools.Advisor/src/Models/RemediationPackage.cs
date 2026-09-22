// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// ARM resource envelope returned by the
/// <c>GET /providers/Microsoft.Advisor/remediations/{recommendationTypeId}</c> operation.
/// </summary>
public sealed record RemediationPackage
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("properties")]
    public RemediationProperties? Properties { get; init; }
}
