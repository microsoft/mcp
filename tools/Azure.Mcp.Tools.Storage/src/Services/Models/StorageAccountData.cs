// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Services.Azure.Models;
using Azure.Mcp.Tools.Storage.Commands;

namespace Azure.Mcp.Tools.Storage.Services.Models;

/// <summary>
/// A class representing the StorageAccount data model.
/// A storage account resource.
/// </summary>
internal sealed class StorageAccountData
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
    /// <summary> The storage account SKU. </summary>
    public ResourceSku? Sku { get; set; }
    /// <summary> Properties of the storage account. </summary>
    public StorageAccountProperties? Properties { get; set; }
    /// <summary> The kind of storage account. </summary>
    public string? Kind { get; set; }

    // Read the JSON response content and create a model instance from it.
    public static StorageAccountData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, StorageJsonContext.Default.StorageAccountData);
    }
}
