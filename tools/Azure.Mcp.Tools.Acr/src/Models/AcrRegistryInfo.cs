// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Acr.Models;

// Lightweight projection of ContainerRegistryData with commonly useful metadata.
// Keep property names stable; only add new nullable properties to extend.
public sealed record AcrRegistryInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("loginServer")] string? LoginServer,
    [property: JsonPropertyName("skuName")] string? SkuName,
    [property: JsonPropertyName("skuTier")] string? SkuTier);
