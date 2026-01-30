// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Models;

namespace Azure.Mcp.Tools.Compute.Services;

/// <summary>
/// Service interface for Azure Compute operations.
/// </summary>
public interface IComputeService
{
    /// <summary>
    /// Gets details of a specific managed disk.
    /// </summary>
    /// <param name="diskName">The name of the disk.</param>
    /// <param name="resourceGroup">The resource group containing the disk.</param>
    /// <param name="subscription">The subscription ID or name.</param>
    /// <param name="tenant">The tenant ID (optional).</param>
    /// <param name="retryPolicy">Retry policy options (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The disk details.</returns>
    Task<Disk> GetDiskAsync(
        string diskName,
        string resourceGroup,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists managed disks in a subscription or resource group.
    /// </summary>
    /// <param name="subscription">The subscription ID or name.</param>
    /// <param name="resourceGroup">The resource group to filter by (optional).</param>
    /// <param name="tenant">The tenant ID (optional).</param>
    /// <param name="retryPolicy">Retry policy options (optional).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of disks.</returns>
    Task<List<Disk>> ListDisksAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
