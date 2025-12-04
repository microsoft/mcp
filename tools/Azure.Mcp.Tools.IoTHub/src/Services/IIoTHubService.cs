// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.IoTHub.Models;

namespace Azure.Mcp.Tools.IoTHub.Services;

public interface IIoTHubService
{
    Task<List<IoTHubDescription>> GetIoTHub(
        string? name,
        string? resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IoTHubDescription> CreateIoTHub(
        string name,
        string resourceGroup,
        string location,
        string sku,
        long capacity,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IoTHubDescription> UpdateIoTHub(
        string name,
        string resourceGroup,
        string? sku,
        long? capacity,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task DeleteIoTHub(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<IoTHubKey>> GetIoTHubKeys(
        string name,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
