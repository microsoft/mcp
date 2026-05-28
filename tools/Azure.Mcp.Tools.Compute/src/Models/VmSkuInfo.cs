// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

/// <summary>
/// Describes a single Azure VM SKU available in a given location, with optional retail pricing.
/// </summary>
public sealed record VmSkuInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("family")] string? Family,
    [property: JsonPropertyName("size")] string? Size,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("vCpus")] int? VCpus,
    [property: JsonPropertyName("memoryGb")] double? MemoryGb,
    [property: JsonPropertyName("maxDataDisks")] int? MaxDataDisks,
    [property: JsonPropertyName("acceleratedNetworking")] bool? AcceleratedNetworking,
    [property: JsonPropertyName("premiumIo")] bool? PremiumIo,
    [property: JsonPropertyName("gpus")] int? Gpus,
    [property: JsonPropertyName("zones")] IReadOnlyList<string>? Zones,
    [property: JsonPropertyName("restrictions")] IReadOnlyList<string>? Restrictions,
    [property: JsonPropertyName("payAsYouGoHourlyUsd")] decimal? PayAsYouGoHourlyUsd,
    [property: JsonPropertyName("spotHourlyUsd")] decimal? SpotHourlyUsd,
    [property: JsonPropertyName("vmScaleSetsSupported")] bool? VMScaleSetsSupported);
