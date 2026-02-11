// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceFabric.Models;

namespace Azure.Mcp.Tools.ServiceFabric.Services;

public interface IServiceFabricService
{
    Task<List<ManagedClusterNode>> ListManagedClusterNodes(
        string subscription,
        string resourceGroup,
        string clusterName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<RestartNodeResponse> RestartManagedClusterNodes(
        string subscription,
        string resourceGroup,
        string clusterName,
        string nodeType,
        string[] nodes,
        string updateType = "Default",
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
