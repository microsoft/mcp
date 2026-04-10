// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmssInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("sku")] VmssSkuInfo? Sku,
    [property: JsonPropertyName("capacity")] long? Capacity,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("upgradePolicy")] string? UpgradePolicy,
    [property: JsonPropertyName("overprovision")] bool? Overprovision,
    [property: JsonPropertyName("zones")] IReadOnlyList<string>? Zones,
    [property: JsonPropertyName("tags")] IReadOnlyDictionary<string, string>? Tags);

public sealed record VmssSkuInfo(
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("tier")] string? Tier,
    [property: JsonPropertyName("capacity")] long? Capacity);
