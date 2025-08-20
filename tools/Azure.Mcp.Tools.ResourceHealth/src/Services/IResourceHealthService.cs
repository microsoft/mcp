// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ResourceHealth.Models;

namespace Azure.Mcp.Tools.ResourceHealth.Services;

public interface IResourceHealthService
{
    /// <summary>
    /// Gets the current availability status of the specified Azure resource.
    /// </summary>
    /// <param name="resourceId">The Azure resource ID</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>The availability status of the resource</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<AvailabilityStatus> GetAvailabilityStatusAsync(
        string resourceId,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Lists availability statuses for all resources in a subscription or resource group.
    /// </summary>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="resourceGroup">Optional resource group name to filter results</param>
    /// <param name="tenant">Optional tenant ID</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <returns>List of availability statuses for resources</returns>
    /// <exception cref="Exception">When the service request fails</exception>
    Task<List<AvailabilityStatus>> ListAvailabilityStatusesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
