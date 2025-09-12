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
            // Use Resource Graph to query AKS clusters
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
            // Use Resource Graph to find the single cluster by name within the specified resource group
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

    // Overload for Resource Graph result
    private static Cluster ConvertToClusterModel(System.Text.Json.JsonElement item)
    {
        var data = Azure.Mcp.Tools.Aks.Services.Models.AksClusterData.FromJson(item);
        if (data == null)
            throw new InvalidOperationException("Failed to parse AKS cluster data");

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
}
