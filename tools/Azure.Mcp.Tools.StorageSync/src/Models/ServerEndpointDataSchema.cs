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
    [property: JsonPropertyName("serverName")] string? ServerName = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("lastOperationName")] string? LastOperationName = null,
    [property: JsonPropertyName("lastWorkflowId")] string? LastWorkflowId = null,
    [property: JsonPropertyName("friendlyName")] string? FriendlyName = null,
    [property: JsonPropertyName("syncStatus")] ServerEndpointSyncStatusSchema? SyncStatus = null,
    [property: JsonPropertyName("cloudTiering")] ServerEndpointCloudTieringSchema? CloudTiering = null,
    [property: JsonPropertyName("offlineDataTransfer")] ServerEndpointOfflineDataTransferSchema? OfflineDataTransfer = null,
    [property: JsonPropertyName("syncPolicies")] ServerEndpointSyncPoliciesSchema? SyncPolicies = null,
    [property: JsonPropertyName("recallStatus")] ServerEndpointRecallStatusSchema? RecallStatus = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public ServerEndpointDataSchema() : this(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null) { }

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
            data.ServerName,
            data.ProvisioningState,
            data.LastOperationName,
            data.LastWorkflowId,
            data.FriendlyName,
            ServerEndpointSyncStatusSchema.FromSdkObject(data.SyncStatus),
            ServerEndpointCloudTieringSchema.FromSdkData(data.CloudTiering, data.VolumeFreeSpacePercent, data.TierFilesOlderThanDays),
            ServerEndpointOfflineDataTransferSchema.FromSdkData(data.OfflineDataTransfer, data.OfflineDataTransferShareName),
            ServerEndpointSyncPoliciesSchema.FromSdkData(data.InitialDownloadPolicy, data.InitialUploadPolicy, data.LocalCacheMode),
            ServerEndpointRecallStatusSchema.FromSdkObject(data.RecallStatus)
        );
    }
}

/// <summary>
/// Sync status information for a server endpoint.
/// </summary>
public sealed record ServerEndpointSyncStatusSchema(
    [property: JsonPropertyName("status")] string? Status = null,
    [property: JsonPropertyName("downloadHealth")] string? DownloadHealth = null,
    [property: JsonPropertyName("uploadHealth")] string? UploadHealth = null,
    [property: JsonPropertyName("combinedHealth")] string? CombinedHealth = null,
    [property: JsonPropertyName("syncActivity")] string? SyncActivity = null,
    [property: JsonPropertyName("totalPersistentFilesNotSyncingCount")] long? TotalPersistentFilesNotSyncingCount = null,
    [property: JsonPropertyName("lastUpdatedOn")] DateTimeOffset? LastUpdatedOn = null)
{
    public static ServerEndpointSyncStatusSchema? FromSdkObject(Azure.ResourceManager.StorageSync.Models.ServerEndpointSyncStatus? sdkStatus)
    {
        if (sdkStatus == null)
            return null;

        return new ServerEndpointSyncStatusSchema(
            null,
            sdkStatus.DownloadHealth?.ToString(),
            sdkStatus.UploadHealth?.ToString(),
            sdkStatus.CombinedHealth?.ToString(),
            sdkStatus.SyncActivity?.ToString(),
            sdkStatus.TotalPersistentFilesNotSyncingCount,
            sdkStatus.LastUpdatedOn
        );
    }
};

/// <summary>
/// Cloud tiering configuration for a server endpoint.
/// </summary>
public sealed record ServerEndpointCloudTieringSchema(
    [property: JsonPropertyName("enabled")] string? Enabled = null,
    [property: JsonPropertyName("volumeFreeSpacePercent")] int? VolumeFreeSpacePercent = null,
    [property: JsonPropertyName("tierFilesOlderThanDays")] int? TierFilesOlderThanDays = null)
{
    public static ServerEndpointCloudTieringSchema? FromSdkData(Azure.ResourceManager.StorageSync.Models.StorageSyncFeatureStatus? cloudTiering, int? volumeFreeSpacePercent, int? tierFilesOlderThanDays)
    {
        if (cloudTiering == null && volumeFreeSpacePercent == null && tierFilesOlderThanDays == null)
            return null;

        return new ServerEndpointCloudTieringSchema(
            cloudTiering?.ToString(),
            volumeFreeSpacePercent,
            tierFilesOlderThanDays
        );
    }
};

/// <summary>
/// Offline data transfer configuration for a server endpoint.
/// </summary>
public sealed record ServerEndpointOfflineDataTransferSchema(
    [property: JsonPropertyName("enabled")] string? Enabled = null,
    [property: JsonPropertyName("shareName")] string? ShareName = null)
{
    public static ServerEndpointOfflineDataTransferSchema? FromSdkData(Azure.ResourceManager.StorageSync.Models.StorageSyncFeatureStatus? offlineDataTransfer, string? shareName)
    {
        if (offlineDataTransfer == null && shareName == null)
            return null;

        return new ServerEndpointOfflineDataTransferSchema(
            offlineDataTransfer?.ToString(),
            shareName
        );
    }
};

/// <summary>
/// Sync policies configuration for a server endpoint.
/// </summary>
public sealed record ServerEndpointSyncPoliciesSchema(
    [property: JsonPropertyName("initialDownloadPolicy")] string? InitialDownloadPolicy = null,
    [property: JsonPropertyName("initialUploadPolicy")] string? InitialUploadPolicy = null,
    [property: JsonPropertyName("localCacheMode")] string? LocalCacheMode = null)
{
    public static ServerEndpointSyncPoliciesSchema? FromSdkData(Azure.ResourceManager.StorageSync.Models.InitialDownloadPolicy? initialDownloadPolicy, Azure.ResourceManager.StorageSync.Models.InitialUploadPolicy? initialUploadPolicy, Azure.ResourceManager.StorageSync.Models.LocalCacheMode? localCacheMode)
    {
        if (initialDownloadPolicy == null && initialUploadPolicy == null && localCacheMode == null)
            return null;

        return new ServerEndpointSyncPoliciesSchema(
            initialDownloadPolicy?.ToString(),
            initialUploadPolicy?.ToString(),
            localCacheMode?.ToString()
        );
    }
};

/// <summary>
/// Recall status information for a server endpoint.
/// </summary>
public sealed record ServerEndpointRecallStatusSchema(
    [property: JsonPropertyName("status")] string? Status = null,
    [property: JsonPropertyName("lastUpdatedOn")] DateTimeOffset? LastUpdatedOn = null,
    [property: JsonPropertyName("totalRecallErrorsCount")] long? TotalRecallErrorsCount = null,
    [property: JsonPropertyName("recallErrors")] IReadOnlyList<ServerEndpointRecallErrorSchema>? RecallErrors = null)
{
    public static ServerEndpointRecallStatusSchema? FromSdkObject(Azure.ResourceManager.StorageSync.Models.ServerEndpointRecallStatus? sdkStatus)
    {
        if (sdkStatus == null)
            return null;

        return new ServerEndpointRecallStatusSchema(
            null,
            sdkStatus.LastUpdatedOn,
            sdkStatus.TotalRecallErrorsCount,
            sdkStatus.RecallErrors?.Select(ServerEndpointRecallErrorSchema.FromSdkObject).OfType<ServerEndpointRecallErrorSchema>().ToList() ?? []
        );
    }
};

/// <summary>
/// Recall error information for a server endpoint.
/// </summary>
public sealed record ServerEndpointRecallErrorSchema(
    [property: JsonPropertyName("errorCode")] long? ErrorCode = null,
    [property: JsonPropertyName("count")] long? Count = null)
{
    public static ServerEndpointRecallErrorSchema? FromSdkObject(Azure.ResourceManager.StorageSync.Models.ServerEndpointRecallError? sdkError)
    {
        if (sdkError == null)
            return null;

        return new ServerEndpointRecallErrorSchema(
            sdkError.ErrorCode,
            sdkError.Count
        );
    }
};
