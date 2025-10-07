// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Aks.Models;

namespace Azure.Mcp.Tools.Aks.Services;

public interface IAksService
{
    Task<List<Cluster>> GetClusters(
        string subscription,
        string? clusterName,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<List<NodePool>> GetNodePools(
        string subscription,
        string resourceGroup,
        string clusterName,
        string? nodePoolName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
