// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A single inline, executable artifact (cli, powershell, bicep, or arm) with its content.
/// </summary>
public sealed record RemediationArtifact
{
    [JsonPropertyName("artifactType")]
    public string? ArtifactType { get; init; }

    [JsonPropertyName("contentType")]
    public string? ContentType { get; init; }

    [JsonPropertyName("confidence")]
    public string? Confidence { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }
}
