// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Dps.Models;

/// <summary>
/// Result of a DPS instance create or update operation.
/// </summary>
/// <param name="HasData">Whether the result has data.</param>
/// <param name="Id">The resource ID of the DPS instance.</param>
/// <param name="Name">The name of the DPS instance.</param>
/// <param name="Type">The resource type.</param>
/// <param name="Location">The Azure region.</param>
/// <param name="SkuName">The SKU name.</param>
/// <param name="SkuTier">The SKU tier.</param>
/// <param name="Properties">Additional properties.</param>
public record DpsInstanceResult(
    [property: JsonPropertyName("hasData")] bool HasData,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("skuName")] string? SkuName,
    [property: JsonPropertyName("skuTier")] string? SkuTier,
    [property: JsonPropertyName("properties")] IDictionary<string, object>? Properties);
