// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Aks.Models;
using Azure.ResourceManager.ContainerService;

namespace Azure.Mcp.Tools.Aks.Services;

public sealed class AksService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ICacheService cacheService) : BaseAzureService(tenantService), IAksService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ICacheService _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));

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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var clusters = new List<Cluster>();

        try
        {
            await foreach (var cluster in subscriptionResource.GetContainerServiceManagedClustersAsync())
            {
                if (cluster?.Data != null)
                {
                    clusters.Add(ConvertToClusterModel(cluster));
                }
            }

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, clusters, s_cacheDuration);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving AKS clusters: {ex.Message}", ex);
        }

        return clusters;
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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        try
        {
            var resourceGroupResource = await subscriptionResource
                .GetResourceGroupAsync(resourceGroup);

            if (resourceGroupResource?.Value == null)
            {
                return null;
            }

            var clusterResource = await resourceGroupResource.Value
                .GetContainerServiceManagedClusters()
                .GetAsync(clusterName);

            if (clusterResource?.Value?.Data == null)
            {
                return null;
            }

            var cluster = ConvertToClusterModel(clusterResource.Value);

            // Cache the result
            await _cacheService.SetAsync(CacheGroup, cacheKey, cluster, s_cacheDuration);

            return cluster;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving AKS cluster '{clusterName}': {ex.Message}", ex);
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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var nodePools = new List<NodePool>();

        try
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            if (resourceGroupResource?.Value == null)
            {
                return nodePools;
            }

            var clusterResource = await resourceGroupResource.Value
                .GetContainerServiceManagedClusters()
                .GetAsync(clusterName);

            if (clusterResource?.Value == null)
            {
                return nodePools;
            }

            await foreach (var agentPool in clusterResource.Value
                               .GetContainerServiceAgentPools()
                               .GetAllAsync())
            {
                if (agentPool?.Data != null)
                {
                    nodePools.Add(ConvertToNodePoolModel(agentPool));
                }
            }

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, nodePools, s_cacheDuration);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving AKS node pools for cluster '{clusterName}': {ex.Message}", ex);
        }

        return nodePools;
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

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        try
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            if (resourceGroupResource?.Value == null)
            {
                return null;
            }

            var clusterResource = await resourceGroupResource.Value
                .GetContainerServiceManagedClusters()
                .GetAsync(clusterName);

            if (clusterResource?.Value == null)
            {
                return null;
            }

            var agentPoolResource = await clusterResource.Value
                .GetContainerServiceAgentPools()
                .GetAsync(nodePoolName);

            if (agentPoolResource?.Value?.Data == null)
            {
                return null;
            }

            var nodePool = ConvertToNodePoolModel(agentPoolResource.Value);

            // Cache the result
            await _cacheService.SetAsync(CacheGroup, cacheKey, nodePool, s_cacheDuration);

            return nodePool;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving AKS node pool '{nodePoolName}' for cluster '{clusterName}': {ex.Message}", ex);
        }
    }

    private static Cluster ConvertToClusterModel(ContainerServiceManagedClusterResource clusterResource)
    {
        var data = clusterResource.Data;
        var agentPool = data.AgentPoolProfiles?.FirstOrDefault();

        return new Cluster
        {
            Name = data.Name,
            SubscriptionId = clusterResource.Id.SubscriptionId,
            ResourceGroupName = clusterResource.Id.ResourceGroupName,
            Location = data.Location.ToString(),
            KubernetesVersion = data.KubernetesVersion,
            ProvisioningState = data.ProvisioningState?.ToString(),
            PowerState = data.PowerStateCode?.ToString(),
            DnsPrefix = data.DnsPrefix,
            Fqdn = data.Fqdn,
            NodeCount = agentPool?.Count,
            NodeVmSize = agentPool?.VmSize,
            IdentityType = data.Identity?.ManagedServiceIdentityType.ToString(),
            EnableRbac = data.EnableRbac,
            NetworkPlugin = data.NetworkProfile?.NetworkPlugin?.ToString(),
            NetworkPolicy = data.NetworkProfile?.NetworkPolicy?.ToString(),
            ServiceCidr = data.NetworkProfile?.ServiceCidr,
            DnsServiceIP = data.NetworkProfile?.DnsServiceIP?.ToString(),
            SkuTier = data.Sku?.Tier?.ToString(),
            Tags = data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };
    }

    private static NodePool ConvertToNodePoolModel(ContainerServiceAgentPoolResource agentPoolResource)
    {
        var data = agentPoolResource.Data;

        return new NodePool
        {
            Name = data.Name,
            Count = data.Count,
            VmSize = data.VmSize?.ToString(),
            OsDiskSizeGB = data.OSDiskSizeInGB,
            OsDiskType = data.OSDiskType?.ToString(),
            KubeletDiskType = data.KubeletDiskType?.ToString(),
            MaxPods = data.MaxPods,
            Type = data.TypePropertiesType?.ToString(),
            MaxCount = data.MaxCount,
            MinCount = data.MinCount,
            EnableAutoScaling = data.EnableAutoScaling,
            ScaleDownMode = data.ScaleDownMode?.ToString(),
            ProvisioningState = data.ProvisioningState?.ToString(),
            PowerState = data.PowerStateCode.HasValue ? new NodePoolPowerState { Code = data.PowerStateCode.Value.ToString() } : null,
            Mode = data.Mode?.ToString(),
            OrchestratorVersion = data.OrchestratorVersion,
            CurrentOrchestratorVersion = data.CurrentOrchestratorVersion,
            EnableNodePublicIP = data.EnableNodePublicIP,
            ScaleSetPriority = data.ScaleSetPriority?.ToString(),
            ScaleSetEvictionPolicy = data.ScaleSetEvictionPolicy?.ToString(),
            NodeLabels = data.NodeLabels?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            NodeTaints = data.NodeTaints?.ToList(),
            OsType = data.OSType?.ToString(),
            OsSKU = data.OSSku?.ToString(),
            NodeImageVersion = data.NodeImageVersion
        };
    }
}
