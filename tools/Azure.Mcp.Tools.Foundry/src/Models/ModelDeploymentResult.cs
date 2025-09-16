// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.Resources.Models;

namespace Azure.Mcp.Tools.Foundry.Models;

public record ModelDeploymentResult
{
    [JsonPropertyName("hasData")]
    public bool HasData { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("type")]
    public string? Type { get; init; }

    [JsonPropertyName("sku")]
    public ResourcesSku? Sku { get; init; }

    [JsonPropertyName("tags")]
    public IDictionary<string, string>? Tags { get; init; }

    [JsonPropertyName("properties")]
    public IDictionary<string, object>? Properties { get; init; }
}

