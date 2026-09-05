// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Remediation metadata, safety flags, inline artifacts, and human-readable methods for a recommendation type.
/// </summary>
public sealed record RemediationProperties
{
    [JsonPropertyName("recommendationTypeId")]
    public string? RecommendationTypeId { get; init; }

    [JsonPropertyName("outputType")]
    public string? OutputType { get; init; }

    [JsonPropertyName("destructive")]
    public bool? Destructive { get; init; }

    [JsonPropertyName("reversible")]
    public bool? Reversible { get; init; }

    [JsonPropertyName("grounded")]
    public bool? Grounded { get; init; }

    [JsonPropertyName("confidence")]
    public string? Confidence { get; init; }

    [JsonPropertyName("version")]
    public int? Version { get; init; }

    [JsonPropertyName("artifacts")]
    public List<RemediationArtifact>? Artifacts { get; init; }

    [JsonPropertyName("methods")]
    public List<RemediationMethod>? Methods { get; init; }
}
