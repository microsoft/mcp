// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Models;

public sealed record StorageAccountResult
{
    [JsonPropertyName("hasData")]
    public bool HasData { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("location")]
    public string? Location { get; init; }

    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    [JsonPropertyName("skuName")]
    public string? SkuName { get; init; }

    [JsonPropertyName("skuTier")]
    public string? SkuTier { get; init; }

    [JsonPropertyName("properties")]
    public IDictionary<string, object>? Properties { get; init; }
}
