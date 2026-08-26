// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A human-readable remediation method (e.g. Azure CLI) with parameters, ordered steps, and verification.
/// </summary>
public sealed record RemediationMethod
{
    [JsonPropertyName("heading")]
    public string? Heading { get; init; }

    [JsonPropertyName("method")]
    public string? Method { get; init; }

    [JsonPropertyName("relation")]
    public string? Relation { get; init; }

    [JsonPropertyName("executable")]
    public bool? Executable { get; init; }

    [JsonPropertyName("parameters")]
    public List<RemediationParameter>? Parameters { get; init; }

    [JsonPropertyName("steps")]
    public List<RemediationStep>? Steps { get; init; }

    [JsonPropertyName("verification")]
    public string? Verification { get; init; }
}
