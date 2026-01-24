// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.StorageSync.Models;

namespace Azure.Mcp.Tools.StorageSync.Services;

/// <summary>
/// Service interface for Storage Sync operations.
/// Defines all methods for managing Azure File Sync resources.
/// </summary>
public interface IStorageSyncService
{
    #region Storage Sync Service Operations

    /// <summary>
    /// Lists all storage sync services in a subscription or resource group.
    /// </summary>
    Task<List<StorageSyncServiceDataSchema>> ListStorageSyncServicesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific storage sync service.
    /// </summary>
    Task<StorageSyncServiceDataSchema?> GetStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new storage sync service.
    /// </summary>
    Task<StorageSyncServiceDataSchema> CreateStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string location,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a storage sync service.
    /// </summary>
    Task<StorageSyncServiceDataSchema> UpdateStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? incomingTrafficPolicy = null,
        Dictionary<string, object>? tags = null,
        string? identityType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a storage sync service.
    /// </summary>
    Task DeleteStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Sync Group Operations

    /// <summary>
    /// Lists all sync groups in a storage sync service.
    /// </summary>
    Task<List<SyncGroupDataSchema>> ListSyncGroupsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific sync group.
    /// </summary>
    Task<SyncGroupDataSchema?> GetSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new sync group.
    /// </summary>
    Task<SyncGroupDataSchema> CreateSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sync group.
    /// </summary>
    Task DeleteSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Cloud Endpoint Operations

    /// <summary>
    /// Lists all cloud endpoints in a sync group.
    /// </summary>
    Task<List<CloudEndpointDataSchema>> ListCloudEndpointsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific cloud endpoint.
    /// </summary>
    Task<CloudEndpointDataSchema?> GetCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new cloud endpoint.
    /// </summary>
    Task<CloudEndpointDataSchema> CreateCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string storageAccountResourceId,
        string azureFileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a cloud endpoint.
    /// </summary>
    Task DeleteCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers change detection on a cloud endpoint.
    /// </summary>
    Task TriggerChangeDetectionAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string directoryPath,
        string? changeDetectionMode = null,
        IList<string>? paths = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Server Endpoint Operations

    /// <summary>
    /// Lists all server endpoints in a sync group.
    /// </summary>
    Task<List<ServerEndpointDataSchema>> ListServerEndpointsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific server endpoint.
    /// </summary>
    Task<ServerEndpointDataSchema?> GetServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new server endpoint.
    /// </summary>
    Task<ServerEndpointDataSchema> CreateServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string serverResourceId,
        string serverLocalPath,
        bool? enableCloudTiering = null,
        int? volumeFreeSpacePercent = null,
        int? tierFilesOlderThanDays = null,
        string? localCacheMode = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a server endpoint's configuration.
    /// </summary>
    Task<ServerEndpointDataSchema> UpdateServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        bool? enableCloudTiering = null,
        int? volumeFreeSpacePercent = null,
        int? tierFilesOlderThanDays = null,
        string? localCacheMode = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a server endpoint.
    /// </summary>
    Task DeleteServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Registered Server Operations

    /// <summary>
    /// Lists all servers registered to a storage sync service.
    /// </summary>
    Task<List<RegisteredServerDataSchema>> ListRegisteredServersAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific registered server.
    /// </summary>
    Task<RegisteredServerDataSchema?> GetRegisteredServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new server to a storage sync service.
    /// </summary>
    Task<RegisteredServerDataSchema> RegisterServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a server from a storage sync service.
    /// </summary>
    Task UnregisterServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a registered server.
    /// </summary>
    Task<RegisteredServerDataSchema> UpdateServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        Dictionary<string, object>? properties = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    #endregion
}
