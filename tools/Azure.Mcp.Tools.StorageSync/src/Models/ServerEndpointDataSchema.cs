// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.StorageSync;

namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Data transfer object for Server Endpoint information.
/// </summary>
public sealed record ServerEndpointDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("serverResourceId")] string? ServerResourceId = null,
    [property: JsonPropertyName("serverLocalPath")] string? ServerLocalPath = null,
    [property: JsonPropertyName("cloudTiering")] string? CloudTiering = null,
    [property: JsonPropertyName("volumeFreeSpacePercent")] int? VolumeFreeSpacePercent = null,
    [property: JsonPropertyName("tierFilesOlderThanDays")] int? TierFilesOlderThanDays = null,
    [property: JsonPropertyName("syncStatus")] string? SyncStatus = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("lastOperationName")] string? LastOperationName = null,
    [property: JsonPropertyName("lastSyncSuccess")] DateTimeOffset? LastSyncSuccess = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public ServerEndpointDataSchema() : this(null, null, null, null, null, null, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a ServerEndpointDataSchema from a StorageSyncServerEndpointResource.
    /// </summary>
    public static ServerEndpointDataSchema FromResource(StorageSyncServerEndpointResource resource)
    {
        var data = resource.Data;
        return new ServerEndpointDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            data.ServerResourceId?.ToString(),
            data.ServerLocalPath,
            data.CloudTiering?.ToString(),
            data.VolumeFreeSpacePercent,
            data.TierFilesOlderThanDays
        );
    }
}
