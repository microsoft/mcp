// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.Grafana.Commands;

namespace Azure.Mcp.Tools.Grafana.Services.Models;

/// <summary>
/// A class representing the Managed Grafana data model.
/// </summary>
internal sealed class ManagedGrafanaData
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
    /// <summary> The identity of the resource, if configured. </summary>
    public ManagedServiceIdentity? Identity { get; set; }
    /// <summary> The availability zones of the resource. </summary>
    public IList<string>? Zones { get; set; }
    /// <summary> Resource tags associated with the resource. </summary>
    public IDictionary<string, string>? Tags { get; set; }
    /// <summary> Properties of the Grafana workspace. </summary>
    public ManagedGrafanaProperties? Properties { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static ManagedGrafanaData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize<ManagedGrafanaData>(source, GrafanaJsonContext.Default.ManagedGrafanaData);
    }
}
