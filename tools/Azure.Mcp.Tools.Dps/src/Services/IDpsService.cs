// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Dps.Models;

namespace Azure.Mcp.Tools.Dps.Services;

/// <summary>
/// Service interface for Device Provisioning Service operations.
/// </summary>
public interface IDpsService
{
    /// <summary>
    /// Lists Device Provisioning Service instances in a subscription.
    /// </summary>
    /// <param name="subscription">The subscription ID or name.</param>
    /// <param name="resourceGroup">Optional resource group to filter instances.</param>
    /// <param name="tenant">Optional tenant ID.</param>
    /// <param name="retryPolicy">Optional retry policy options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of DPS instances.</returns>
    Task<List<DpsInstanceInfo>> ListInstancesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Device Provisioning Service instance.
    /// </summary>
    /// <param name="instanceName">The name of the DPS instance to create.</param>
    /// <param name="resourceGroup">The resource group name.</param>
    /// <param name="location">The Azure region for the DPS instance.</param>
    /// <param name="subscription">The subscription ID or name.</param>
    /// <param name="sku">Optional SKU name (default: S1).</param>
    /// <param name="capacity">Optional capacity (number of units, default: 1).</param>
    /// <param name="allocationPolicy">Optional allocation policy (default: Hashed).</param>
    /// <param name="linkedHubConnectionString">Optional connection string of the IoT Hub to link.</param>
    /// <param name="linkedHubLocation">Optional location of the linked IoT Hub.</param>
    /// <param name="tenant">Optional tenant ID.</param>
    /// <param name="retryPolicy">Optional retry policy options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created DPS instance result.</returns>
    Task<DpsInstanceResult> CreateInstanceAsync(
        string instanceName,
        string resourceGroup,
        string location,
        string subscription,
        string? sku = null,
        int? capacity = null,
        string? allocationPolicy = null,
        string? linkedHubConnectionString = null,
        string? linkedHubLocation = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
