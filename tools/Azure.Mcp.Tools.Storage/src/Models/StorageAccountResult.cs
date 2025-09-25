// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Models;

public sealed record StorageAccountResult(
    [property: JsonPropertyName("hasData")] bool HasData,
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("kind")] string? Kind,
    [property: JsonPropertyName("skuName")] string? SkuName,
    [property: JsonPropertyName("skuTier")] string? SkuTier,
    [property: JsonPropertyName("properties")] IDictionary<string, object>? Properties);
