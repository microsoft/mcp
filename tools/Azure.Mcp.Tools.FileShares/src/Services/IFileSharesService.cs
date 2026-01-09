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
    Task<bool> CheckNameAvailabilityAsync(
        string subscription,
        string fileShareName,
        string location,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List snapshots of a file share.
    /// </summary>
    Task<List<FileShareSnapshotInfo>> ListFileShareSnapshotsAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get details of a file share snapshot.
    /// </summary>
    Task<FileShareSnapshotInfo> GetFileShareSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a snapshot of a file share.
    /// </summary>
    Task<FileShareSnapshotInfo> CreateFileShareSnapshotAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List private endpoint connections for a file share.
    /// </summary>
    Task<List<PrivateEndpointConnectionInfo>> ListPrivateEndpointConnectionsAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get details of a private endpoint connection.
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
    /// Update the approval state of a private endpoint connection.
    /// </summary>
    Task<PrivateEndpointConnectionInfo> UpdatePrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string status,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a private endpoint connection.
    /// </summary>
    Task DeletePrivateEndpointConnectionAsync(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string connectionName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
