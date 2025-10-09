// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.Kusto.Commands;

namespace Azure.Mcp.Tools.Kusto.Services.Models;

/// <summary>
/// A class representing the Kusto Cluster data model.
/// </summary>
internal sealed class KustoClusterData
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
    /// <summary> Properties of the Kusto cluster. </summary>
    public KustoClusterProperties? Properties { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static KustoClusterData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize<KustoClusterData>(source, KustoJsonContext.Default.KustoClusterData);
    }
}
