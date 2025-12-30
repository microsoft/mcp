// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.StorageSync;

namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Data transfer object for Storage Sync service information.
/// </summary>
public sealed record StorageSyncServiceDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("location")] string? Location = null,
    [property: JsonPropertyName("tags")] Dictionary<string, string>? Tags = null,
    [property: JsonPropertyName("properties")] StorageSyncServicePropertiesSchema? Properties = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public StorageSyncServiceDataSchema() : this(null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a StorageSyncServiceDataSchema from a StorageSyncServiceResource.
    /// </summary>
    public static StorageSyncServiceDataSchema FromResource(StorageSyncServiceResource resource)
    {
        var data = resource.Data;
        return new StorageSyncServiceDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            data.Location.ToString(),
            new Dictionary<string, string>(data.Tags ?? new Dictionary<string, string>()),
            new StorageSyncServicePropertiesSchema(data.IncomingTrafficPolicy?.ToString())
        );
    }
}

/// <summary>
/// Storage Sync service properties.
/// </summary>
public sealed record StorageSyncServicePropertiesSchema(
    [property: JsonPropertyName("incomingTrafficPolicy")] string? IncomingTrafficPolicy = null)
{
}
