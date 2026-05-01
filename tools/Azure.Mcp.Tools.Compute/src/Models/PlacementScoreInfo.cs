// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Represents a single placement score result for a VM SKU in a specific region/zone.
/// </summary>
public sealed record PlacementScoreInfo(
    [property: JsonPropertyName("sku")] string? Sku,
    [property: JsonPropertyName("region")] string? Region,
    [property: JsonPropertyName("availabilityZone")] string? AvailabilityZone,
    [property: JsonPropertyName("score")] string? Score,
    [property: JsonPropertyName("isQuotaAvailable")] bool? IsQuotaAvailable);
