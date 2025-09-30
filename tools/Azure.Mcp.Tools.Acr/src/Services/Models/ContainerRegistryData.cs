// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.Acr.Commands;

namespace Azure.Mcp.Tools.Acr.Services.Models;

/// <summary>
/// A class representing the ContainerRegistryData data model.
/// </summary>
internal sealed class ContainerRegistryData
{
    /// <summary> The resource ID for the resource. </summary>
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }
    /// <summary> The type of the resource. </summary>
    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }
    /// <summary> The name of the resource. </summary>
    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }
    /// <summary> The location of the resource. </summary>
    public string? Location { get; set; }
    /// <summary> The SKU of the resource. </summary>
    public ResourceSku? Sku { get; set; }
    /// <summary> Properties of the Container Registry. </summary>
    public ContainerRegistryProperties? Properties { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static ContainerRegistryData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize<ContainerRegistryData>(source, AcrJsonContext.Default.ContainerRegistryData);
    }
}
