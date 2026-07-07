// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.DeviceRegistry.Commands;

namespace Azure.Mcp.Tools.DeviceRegistry.Services.Models;

/// <summary>
/// A class representing the Device Registry Namespace data model from Resource Graph.
/// </summary>
internal sealed class DeviceRegistryNamespaceData
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

    /// <summary> The resource group of the resource. </summary>
    public string? ResourceGroup { get; set; }

    /// <summary> Properties of the Device Registry Namespace. </summary>
    public DeviceRegistryNamespaceProperties? Properties { get; set; }

    public static DeviceRegistryNamespaceData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, DeviceRegistryJsonContext.Default.DeviceRegistryNamespaceData);
    }
}
