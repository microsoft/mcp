// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Models;

// Lightweight projection of StorageAccountData with commonly useful metadata.
// Keep property names stable; only add new nullable properties to extend.
public sealed record AccountInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("kind")] string? Kind,
    [property: JsonPropertyName("skuName")] string? SkuName,
    [property: JsonPropertyName("skuTier")] string? SkuTier,
    [property: JsonPropertyName("hnsEnabled")] bool? IsHnsEnabled,
    [property: JsonPropertyName("allowBlobPublicAccess")] bool? AllowBlobPublicAccess,
    [property: JsonPropertyName("enableHttpsTrafficOnly")] bool? EnableHttpsTrafficOnly);
