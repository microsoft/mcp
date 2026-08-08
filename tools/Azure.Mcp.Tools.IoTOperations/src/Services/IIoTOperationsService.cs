// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.IoTOperations.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.IoTOperations.Services;

public interface IIoTOperationsService
{
    Task<ResourceQueryResults<IoTOperationsInstanceInfo>> ListInstancesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<IoTOperationsInstanceInfo> GetInstanceAsync(
        string subscription,
        string resourceGroup,
        string instance,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
