// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models;
using Azure.Mcp.Tools.FileShares.Models;

namespace Azure.Mcp.Tools.FileShares.Services;

/// <summary>
/// Service interface for Azure File Shares operations.
/// </summary>
public interface IFileSharesService
{
    /// <summary>
    /// List file shares in a subscription or resource group.
    /// </summary>
    Task<List<FileShareInfo>> ListFileSharesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get details of a specific file share.
    /// </summary>
    Task<FileShareInfo> GetFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create or update a file share.
    /// </summary>
    Task<FileShareInfo> CreateOrUpdateFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string location,
        string? mountName = null,
        string? mediaTier = null,
        string? redundancy = null,
        string? protocol = null,
        int? provisionedStorageInGiB = null,
        int? provisionedIOPerSec = null,
        int? provisionedThroughputMiBPerSec = null,
        string? publicNetworkAccess = null,
        string? nfsRootSquash = null,
        string[]? allowedSubnets = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Patch (update) an existing file share by only modifying specified properties.
    /// Fetches the existing file share and updates only the provided properties.
    /// </summary>
    Task<FileShareInfo> PatchFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        int? provisionedStorageInGiB = null,
        int? provisionedIOPerSec = null,
        int? provisionedThroughputMiBPerSec = null,
        string? publicNetworkAccess = null,
        string? nfsRootSquash = null,
        string[]? allowedSubnets = null,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file share.
    /// </summary>
    Task DeleteFileShareAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a file share name is available.
    /// </summary>
    Task<FileShareNameAvailabilityResult> CheckNameAvailabilityAsync(
        string subscription,
        string fileShareName,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a snapshot of a file share.
    /// </summary>
    Task<FileShareSnapshotInfo> CreateSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotName,
        Dictionary<string, string>? metadata = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get details of a file share snapshot.
    /// </summary>
    Task<FileShareSnapshotInfo> GetSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List snapshots of a file share.
    /// </summary>
    Task<List<FileShareSnapshotInfo>> ListSnapshotsAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a file share snapshot.
    /// </summary>
    Task<FileShareSnapshotInfo> PatchSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        Dictionary<string, string>? metadata = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file share snapshot.
    /// </summary>
    Task DeleteSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// Get file share limits for a subscription and location.
    /// </summary>
    Task<FileShareLimitsResult> GetLimitsAsync(
        string subscription,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file share usage data for a subscription and location.
    /// </summary>
    Task<FileShareUsageDataResult> GetUsageDataAsync(
        string subscription,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get provisioning recommendations for a file share based on desired storage size.
    /// </summary>
    Task<FileShareProvisioningRecommendationResult> GetProvisioningRecommendationAsync(
        string subscription,
        string location,
        int provisionedStorageGiB,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
    /// <summary>
    /// Get a specific private endpoint connection for a file share.
    /// </summary>
    Task<PrivateEndpointConnectionInfo> GetPrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List all private endpoint connections for a file share.
    /// </summary>
    Task<List<PrivateEndpointConnectionInfo>> ListPrivateEndpointConnectionsAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update the state of a private endpoint connection.
    /// </summary>
    Task<PrivateEndpointConnectionInfo> UpdatePrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string status,
        string? description = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);}
