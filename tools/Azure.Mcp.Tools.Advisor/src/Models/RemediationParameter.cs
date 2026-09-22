// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// A parameter required by a remediation method.
/// </summary>
public sealed record RemediationParameter
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("example")]
    public string? Example { get; init; }

    [JsonPropertyName("required")]
    public bool? Required { get; init; }
}
