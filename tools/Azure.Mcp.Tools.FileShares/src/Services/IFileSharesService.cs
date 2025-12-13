// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FileShares.Models;

namespace Azure.Mcp.Tools.FileShares.Services;

public interface IFileSharesService
{
    /// <summary>
    /// Lists all file shares in a subscription or resource group.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">Optional resource group name to filter results</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>List of file share names</returns>
    Task<List<string>> ListFileShares(
        string subscription,
        string? resourceGroup = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific file share by name.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group containing the file share</param>
    /// <param name="name">The name of the file share</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>File share details</returns>
    Task<FileShareDetail> GetFileShare(
        string subscription,
        string resourceGroup,
        string name,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file share name is available in a location.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="location">The Azure region to check</param>
    /// <param name="name">The proposed file share name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Name availability result</returns>
    Task<NameAvailabilityResult> CheckNameAvailability(
        string subscription,
        string location,
        string name,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file share.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group containing the file share</param>
    /// <param name="name">The name of the file share to delete</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    Task DeleteFileShare(
        string subscription,
        string resourceGroup,
        string name,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all snapshots for a file share.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group containing the file share</param>
    /// <param name="fileShareName">The name of the file share</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>List of snapshot names</returns>
    Task<List<string>> ListFileShareSnapshots(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific file share snapshot.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group containing the file share</param>
    /// <param name="fileShareName">The name of the parent file share</param>
    /// <param name="snapshotName">The name of the snapshot</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Snapshot details</returns>
    Task<FileShareSnapshot> GetFileShareSnapshot(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string snapshotName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a snapshot of a file share.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group containing the file share</param>
    /// <param name="fileShareName">The name of the file share</param>
    /// <param name="snapshotName">The name for the new snapshot</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Created snapshot details</returns>
    Task<FileShareSnapshot> CreateFileShareSnapshot(
        string subscription,
        string resourceGroup,
        string fileShareName,
        string? snapshotName = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all private endpoint connections for a file share resource.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>List of private endpoint connections</returns>
    Task<List<PrivateEndpointConnectionData>> ListPrivateEndpointConnections(
        string subscription,
        string resourceGroup,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific private endpoint connection.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="connectionName">The name of the private endpoint connection</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Private endpoint connection details</returns>
    Task<PrivateEndpointConnectionData?> GetPrivateEndpointConnection(
        string subscription,
        string resourceGroup,
        string connectionName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the approval status of a private endpoint connection.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="connectionName">The name of the private endpoint connection</param>
    /// <param name="status">The connection status (Approved or Rejected)</param>
    /// <param name="description">Optional reason for approval/rejection</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>Updated private endpoint connection</returns>
    Task<PrivateEndpointConnectionData?> UpdatePrivateEndpointConnection(
        string subscription,
        string resourceGroup,
        string connectionName,
        string status,
        string? description = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a private endpoint connection.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="connectionName">The name of the private endpoint connection to delete</param>
    /// <param name="tenantId">Optional tenant ID for cross-tenant operations</param>
    /// <param name="retryPolicy">Optional retry policy for the operation</param>
    /// <param name="cancellationToken">A cancellation token</param>
    Task DeletePrivateEndpointConnection(
        string subscription,
        string resourceGroup,
        string connectionName,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
