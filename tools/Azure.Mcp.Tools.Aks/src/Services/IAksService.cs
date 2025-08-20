// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Aks.Models;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Aks.Services;

public interface IAksService
{
    Task<List<Cluster>> ListClusters(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);

    Task<Cluster?> GetCluster(
        string subscription,
        string clusterName,
        string resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
