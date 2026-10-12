// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A verification check for a remediation method: a human-readable assertion and the command that validates it.
/// </summary>
public sealed record RemediationCheck
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("command")]
    public string? Command { get; init; }
}
