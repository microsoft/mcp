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
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }

    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    public DeviceRegistryNamespaceProperties? Properties { get; set; }

    public static DeviceRegistryNamespaceData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, DeviceRegistryJsonContext.Default.DeviceRegistryNamespaceData);
    }
}

/// <summary>
/// Properties of a Device Registry Namespace.
/// </summary>
internal sealed class DeviceRegistryNamespaceProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("uuid")]
    public string? Uuid { get; set; }
}
