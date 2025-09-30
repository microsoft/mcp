// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.AppConfig.Commands;

namespace Azure.Mcp.Tools.AppConfig.Services.Models;

/// <summary>
/// A class representing the AppConfigurationStore data model.
/// </summary>
internal sealed class AppConfigurationStoreData
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
    /// <summary> The identity of the cluster, if configured. </summary>
    public ManagedServiceIdentity? Identity { get; set; }
    /// <summary> The availability zones of the cluster. </summary>
    public IList<string>? Zones { get; set; }
    /// <summary> A unique read-only string that changes whenever the resource is updated. </summary>
    [JsonPropertyName("etag")]
    public string? ETag { get; set; }
    /// <summary> The tags of the resource. </summary>
    public IDictionary<string, string>? Tags { get; set; }
    /// <summary> Properties of the App Configuration Store. </summary>
    public AppConfigurationStoreProperties? Properties { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static AppConfigurationStoreData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize<AppConfigurationStoreData>(source, AppConfigJsonContext.Default.AppConfigurationStoreData);
    }
}
