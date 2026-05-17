// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// A ranked Azure region suggestion for VM/VMSS placement based on workload hints.
/// </summary>
public sealed record VmRegionRecommendation(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("geography")] string? Geography,
    [property: JsonPropertyName("physicalLocation")] string? PhysicalLocation,
    [property: JsonPropertyName("availabilityZones")] bool AvailabilityZones,
    [property: JsonPropertyName("score")] int Score,
    [property: JsonPropertyName("rationale")] string Rationale);
