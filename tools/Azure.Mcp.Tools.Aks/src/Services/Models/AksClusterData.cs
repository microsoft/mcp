// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands;

namespace Azure.Mcp.Tools.Aks.Services.Models;

/// <summary>
/// A class representing the AKS Cluster data model for Resource Graph results.
/// </summary>
internal sealed class AksClusterData
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
    public AksManagedClusterSku? Sku { get; set; }
    /// <summary> The identity type of the resource. </summary>
    public string? IdentityType { get; set; }
    /// <summary> The tags of the resource. </summary>
    public IDictionary<string, string>? Tags { get; set; }
    /// <summary> The properties of the cluster. </summary>
    public AksClusterProperties? Properties { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static AksClusterData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize<AksClusterData>(source, AksJsonContext.Default.AksClusterData);
    }
}
