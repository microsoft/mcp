// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.EventHubs.Commands;

namespace Azure.Mcp.Tools.EventHubs.Models;

/// <summary>
/// A class representing the EventHubsNamespace data model.
/// A namespace resource.
/// </summary>
internal sealed class EventHubsNamespaceData
{
    /// <summary> The resource ID for the resource. </summary>
    [JsonPropertyName(EventHubsModelDefinitions.ResourceId)]
    public required string ResourceId { get; set; }
    /// <summary> The type of the resource. </summary>
    [JsonPropertyName(EventHubsModelDefinitions.ResourceType)]
    public required string ResourceType { get; set; }
    /// <summary> The name of the resource. </summary>
    [JsonPropertyName(EventHubsModelDefinitions.ResourceName)]
    public required string ResourceName { get; set; }
    /// <summary> The location of the resource. </summary>
    public required string Location { get; set; }
    /// <summary> Resource tags. </summary>
    public IDictionary<string, string>? Tags { get; set; }
    /// <summary> The database SKU. </summary>
    public required ResourceSku Sku { get; set; }
    /// <summary> Properties of the Event Hubs namespace. </summary>
    public EventHubsNamespaceProperties? Properties { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static EventHubsNamespaceData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, EventHubsJsonContext.Default.EventHubsNamespaceData);
    }
}
