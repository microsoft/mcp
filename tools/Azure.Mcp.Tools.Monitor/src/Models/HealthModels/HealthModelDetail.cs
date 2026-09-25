// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Monitor.Models.HealthModels;

/// <summary>
/// Azure Monitor Health Model with its identity and tags and a health state.
/// <see cref="HealthState"/> is null when the root entity's health cannot be retrieved.
/// </summary>
public sealed class HealthModelDetail : HealthModelSummary
{
    [JsonPropertyName("healthState")]
    public string? HealthState { get; set; }

    [JsonPropertyName("identity")]
    public HealthModelIdentity? Identity { get; set; }

    [JsonPropertyName("tags")]
    public IDictionary<string, string>? Tags { get; set; }
}
