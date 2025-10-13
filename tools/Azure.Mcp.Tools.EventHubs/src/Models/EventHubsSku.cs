// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.EventHubs.Models;

public record EventHubsSku(
    [property: JsonPropertyName(EventHubsModelDefinitions.SkuName)] string? Name,
    [property: JsonPropertyName(EventHubsModelDefinitions.SkuTier)] string? Tier,
    [property: JsonPropertyName(EventHubsModelDefinitions.SkuCapacity)] int? Capacity
);
