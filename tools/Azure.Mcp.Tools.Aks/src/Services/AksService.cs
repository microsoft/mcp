// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Aks.Models;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Azure.Mcp.Tools.Aks.Services;

public sealed class AksService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService,
    ILogger<AksService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IAksService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    private readonly ILogger<AksService> _logger = logger;

    private const string CacheGroup = "aks";
    private const string AksClustersCacheKey = "clusters";
    private const string AksNodePoolsCacheKey = "nodepools";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(1);

    public async Task<List<Cluster>> ListClusters(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        // Create cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{AksClustersCacheKey}_{subscription}"
            : $"{AksClustersCacheKey}_{subscription}_{tenant}";

        // Try to get from cache first
        var cachedClusters = await _cacheService.GetAsync<List<Cluster>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedClusters != null)
        {
            return cachedClusters;
        }

        try
        {
            var clusters = await ExecuteResourceQueryAsync(
                "Microsoft.ContainerService/managedClusters",
                resourceGroup: null, // all resource groups
                subscription,
                retryPolicy,
                ConvertToClusterModel,
                cancellationToken: default);

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, clusters, s_cacheDuration);
            return clusters;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving AKS clusters: {ex.Message}", ex);
        }
    }

    public async Task<Cluster?> GetCluster(
        string subscription,
        string clusterName,
        string resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, clusterName, resourceGroup);

        // Create cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"cluster_{subscription}_{resourceGroup}_{clusterName}"
            : $"cluster_{subscription}_{resourceGroup}_{clusterName}_{tenant}";

        // Try to get from cache first
        var cachedCluster = await _cacheService.GetAsync<Cluster>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedCluster != null)
        {
            return cachedCluster;
        }

        try
        {
            var cluster = await ExecuteSingleResourceQueryAsync(
                "Microsoft.ContainerService/managedClusters",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToClusterModel,
                $"name =~ '{EscapeKqlString(clusterName)}'");

            if (cluster == null)
            {
                throw new KeyNotFoundException($"AKS cluster '{clusterName}' not found in resource group '{resourceGroup}' for subscription '{subscription}'.");
            }

            // Cache the result
            await _cacheService.SetAsync(CacheGroup, cacheKey, cluster, s_cacheDuration);

            return cluster;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving AKS cluster '{ClusterName}' in resource group '{ResourceGroup}' for subscription '{Subscription}'",
                clusterName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<List<NodePool>> ListNodePools(
        string subscription,
        string resourceGroup,
        string clusterName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, clusterName);

        // Create cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"{AksNodePoolsCacheKey}_{subscription}_{resourceGroup}_{clusterName}"
            : $"{AksNodePoolsCacheKey}_{subscription}_{resourceGroup}_{clusterName}_{tenant}";

        // Try to get from cache first
        var cachedNodePools = await _cacheService.GetAsync<List<NodePool>>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedNodePools != null)
        {
            return cachedNodePools;
        }

        try
        {
            var nodePools = await ExecuteSingleResourceQueryAsync(
                "Microsoft.ContainerService/managedClusters",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToClusterNodePoolModel,
                $"name =~ '{EscapeKqlString(clusterName)}'") ?? new List<NodePool>();

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, nodePools, s_cacheDuration);
            return nodePools;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving AKS node pools for cluster '{clusterName}': {ex.Message}", ex);
        }
    }

    public async Task<NodePool?> GetNodePool(
        string subscription,
        string resourceGroup,
        string clusterName,
        string nodePoolName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup, clusterName, nodePoolName);

        // Create cache key
        var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"nodepool_{subscription}_{resourceGroup}_{clusterName}_{nodePoolName}"
            : $"nodepool_{subscription}_{resourceGroup}_{clusterName}_{nodePoolName}_{tenant}";

        // Try to get from cache first
        var cachedNodePool = await _cacheService.GetAsync<NodePool>(CacheGroup, cacheKey, s_cacheDuration);
        if (cachedNodePool != null)
        {
            return cachedNodePool;
        }

        try
        {
            var nodePools = await ExecuteSingleResourceQueryAsync(
                "Microsoft.ContainerService/managedClusters",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToClusterNodePoolModel,
                $"name =~ '{EscapeKqlString(clusterName)}'") ?? new List<NodePool>();

            var nodePool = nodePools.FirstOrDefault(np => np.Name == nodePoolName);
            if (nodePool != null)
            {
                // Cache the result
                await _cacheService.SetAsync(CacheGroup, cacheKey, nodePool, s_cacheDuration);
                return nodePool;
            }
            
            throw new KeyNotFoundException($"AKS node pool '{nodePoolName}' not found in cluster '{clusterName}' in resource group '{resourceGroup}' for subscription '{subscription}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error retrieving AKS node pool '{NodePoolName}' for cluster '{ClusterName}' in resource group '{ResourceGroup}' for subscription '{Subscription}'",
                nodePoolName, clusterName, resourceGroup, subscription);
            throw;
        }
    }

    private static Cluster ConvertToClusterModel(JsonElement item)
    {
        var data = Models.AksClusterData.FromJson(item) ?? throw new InvalidOperationException("Failed to parse AKS cluster data");

        // Resource identity
        if (string.IsNullOrEmpty(data.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(data.ResourceId);

        var agentPool = data.Properties?.AgentPoolProfiles?.FirstOrDefault();
        return new Cluster
        {
            Name = data.ResourceName ?? "Unknown",
            SubscriptionId = id.SubscriptionId ?? "Unknown",
            ResourceGroupName = id.ResourceGroupName ?? "Unknown",
            Location = data.Location ?? "Unknown",
            KubernetesVersion = data.Properties?.KubernetesVersion,
            ProvisioningState = data.Properties?.ProvisioningState,
            PowerState = data.Properties?.PowerState?.Code,
            DnsPrefix = data.Properties?.DnsPrefix,
            Fqdn = data.Properties?.Fqdn,
            NodeCount = agentPool?.Count,
            NodeVmSize = agentPool?.VmSize,
            IdentityType = data.IdentityType,
            EnableRbac = data.Properties?.EnableRbac,
            NetworkPlugin = data.Properties?.NetworkProfile?.NetworkPlugin,
            NetworkPolicy = data.Properties?.NetworkProfile?.NetworkPolicy,
            ServiceCidr = data.Properties?.NetworkProfile?.ServiceCidr,
            DnsServiceIP = data.Properties?.NetworkProfile?.DnsServiceIP,
            SkuTier = data.Sku?.Tier,
            Tags = data.Tags != null ? new Dictionary<string, string>(data.Tags) : null
        };
    }

    private static List<NodePool> ConvertToClusterNodePoolModel(JsonElement item)
    {
        var data = Models.AksClusterData.FromJson(item) ?? throw new InvalidOperationException("Failed to parse AKS cluster data");

        return data.Properties?.AgentPoolProfiles?
            .Select(node => new NodePool
            {
                Name = node.Name ?? "Unknown",
                NodeCount = node.Count,
                NodeVmSize = node.VmSize,
                OsType = node.OSType,
                Mode = node.Mode,
                OrchestratorVersion = node.OrchestratorVersion,
                EnableAutoScaling = node.EnableAutoScaling,
                MinCount = node.MinCount,
                MaxCount = node.MaxCount,
                ProvisioningState = node.ProvisioningState
            })
            .ToList()
            ?? new List<NodePool>();
    }
}
