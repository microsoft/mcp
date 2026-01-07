// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.StorageSync;

namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Data transfer object for Sync Group information.
/// </summary>
public sealed record SyncGroupDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("uniqueId")] string? UniqueId = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public SyncGroupDataSchema() : this(null, null, null, null) { }

    /// <summary>
    /// Creates a SyncGroupDataSchema from a StorageSyncGroupResource.
    /// </summary>
    public static SyncGroupDataSchema FromResource(StorageSyncGroupResource resource)
    {
        var data = resource.Data;
        return new SyncGroupDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            data.UniqueId?.ToString()
        );
    }
}
