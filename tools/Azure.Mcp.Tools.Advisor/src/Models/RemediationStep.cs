// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A single ordered step within a remediation method.
/// </summary>
public sealed record RemediationStep
{
    [JsonPropertyName("number")]
    public int? Number { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("command")]
    public string? Command { get; init; }
}
