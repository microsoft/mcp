// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Aks.Commands;
using Azure.Mcp.Tools.Aks.Models;
using Azure.ResourceManager.ContainerService;
using Azure.ResourceManager.ContainerService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Core.Services.Caching;

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
    private static readonly TimeSpan s_cacheDuration = CacheDurations.ServiceData;

    public async Task<List<Cluster>> GetClusters(
        string subscription,
        string? clusterName,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        if (string.IsNullOrEmpty(clusterName))
        {
            // Create cache key
            var cacheKey = (string.IsNullOrEmpty(resourceGroup), string.IsNullOrEmpty(tenant)) switch
            {
                (true, true) => CacheKeyBuilder.Build(AksClustersCacheKey, subscription),
                (false, true) => CacheKeyBuilder.Build(AksClustersCacheKey, subscription, resourceGroup),
                (true, false) => CacheKeyBuilder.Build(AksClustersCacheKey, subscription, tenant),
                (false, false) => CacheKeyBuilder.Build(AksClustersCacheKey, subscription, resourceGroup, tenant)
            };

            // Try to get from cache first
            var cachedClusters = await _cacheService.GetAsync<List<Cluster>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedClusters != null)
            {
                return cachedClusters;
            }

            var result = await ExecuteResourceQueryAsync(
                "Microsoft.ContainerService/managedClusters",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToClusterFromJson,
                tenant: tenant,
                cancellationToken: cancellationToken);

            var clusters = result.Results;

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, clusters, s_cacheDuration, cancellationToken);

            return clusters;
        }
        else
        {
            ValidateRequiredParameters((nameof(clusterName), clusterName));

            // Create cache key
            var cacheKey = (string.IsNullOrEmpty(resourceGroup), string.IsNullOrEmpty(tenant)) switch
            {
                (true, true) => CacheKeyBuilder.Build("cluster", subscription, clusterName),
                (false, true) => CacheKeyBuilder.Build("cluster", subscription, resourceGroup, clusterName),
                (true, false) => CacheKeyBuilder.Build("cluster", subscription, clusterName, tenant),
                (false, false) => CacheKeyBuilder.Build("cluster", subscription, resourceGroup, clusterName, tenant)
            };

            // Try to get from cache first
            var cachedCluster = await _cacheService.GetAsync<List<Cluster>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedCluster != null)
            {
                return cachedCluster;
            }

            var cluster = await ExecuteSingleResourceQueryAsync(
                "Microsoft.ContainerService/managedClusters",
                resourceGroup: resourceGroup,
                subscription: subscription,
                retryPolicy: retryPolicy,
                converter: ConvertToClusterFromJson,
                additionalFilter: $"name =~ '{EscapeKqlString(clusterName!)}'",
                tenant: tenant,
                cancellationToken: cancellationToken);

            if (cluster == null)
            {
                return [];
            }

            var clusters = new List<Cluster>() { cluster };

            // Cache the result
            await _cacheService.SetAsync(CacheGroup, cacheKey, clusters, s_cacheDuration, cancellationToken);

            return clusters;
        }
    }

    public async Task<List<NodePool>> GetNodePools(
        string subscription,
        string resourceGroup,
        string clusterName,
        string? nodePoolName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(clusterName), clusterName));

        if (string.IsNullOrEmpty(nodePoolName))
        {
            // Create cache key
            var cacheKey = string.IsNullOrEmpty(tenant)
                ? CacheKeyBuilder.Build(AksNodePoolsCacheKey, subscription, resourceGroup, clusterName)
                : CacheKeyBuilder.Build(AksNodePoolsCacheKey, subscription, resourceGroup, clusterName, tenant);

            // Try to get from cache first
            var cachedNodePools = await _cacheService.GetAsync<List<NodePool>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedNodePools != null)
            {
                return cachedNodePools;
            }

            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);

            var nodePools = new List<NodePool>();
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            if (resourceGroupResource?.Value == null)
            {
                return nodePools;
            }

            var clusterResource = await resourceGroupResource.Value
                .GetContainerServiceManagedClusters()
                .GetAsync(clusterName, cancellationToken);

            if (clusterResource?.Value == null)
            {
                return nodePools;
            }

            await foreach (var agentPool in clusterResource.Value
                                .GetContainerServiceAgentPools()
                                .GetAllAsync(cancellationToken))
            {
                if (agentPool?.Data != null)
                {
                    nodePools.Add(ConvertToNodePoolModel(agentPool));
                }
            }

            // Cache the results
            await _cacheService.SetAsync(CacheGroup, cacheKey, nodePools, s_cacheDuration, cancellationToken);

            return nodePools;
        }
        else
        {
            // Create cache key
            var cacheKey = string.IsNullOrEmpty(tenant)
                ? CacheKeyBuilder.Build("nodepool", subscription, resourceGroup, clusterName, nodePoolName)
                : CacheKeyBuilder.Build("nodepool", subscription, resourceGroup, clusterName, nodePoolName, tenant);

            // Try to get from cache first
            var cachedNodePool = await _cacheService.GetAsync<List<NodePool>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedNodePool != null)
            {
                return cachedNodePool;
            }

            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            if (resourceGroupResource?.Value == null)
            {
                return [];
            }

            var clusterResource = await resourceGroupResource.Value
                .GetContainerServiceManagedClusters()
                .GetAsync(clusterName, cancellationToken);

            if (clusterResource?.Value == null)
            {
                return [];
            }

            var agentPoolResource = await clusterResource.Value
                .GetContainerServiceAgentPools()
                .GetAsync(nodePoolName, cancellationToken);

            if (agentPoolResource?.Value?.Data == null)
            {
                return [];
            }

            var nodePools = new List<NodePool>() { ConvertToNodePoolModel(agentPoolResource.Value) };

            // Cache the result
            await _cacheService.SetAsync(CacheGroup, cacheKey, nodePools, s_cacheDuration, cancellationToken);

            return nodePools;
        }
    }

    private static Cluster ConvertToClusterFromJson(JsonElement item)
    {
        var response = JsonSerializer.Deserialize(item, AksJsonContext.Default.AksClusterResourceGraphResponse);
        return response is null ? new Cluster() : MapToCluster(response);
    }

    private static Cluster MapToCluster(AksClusterResourceGraphResponse response)
    {
        var props = response.Properties;
        var np = props?.NetworkProfile;

        var cluster = new Cluster
        {
            Id = response.Id,
            Name = response.Name,
            SubscriptionId = response.SubscriptionId,
            ResourceGroupName = response.ResourceGroup,
            Location = response.Location,
            Tags = response.Tags,
            SkuName = response.Sku?.Name,
            SkuTier = response.Sku?.Tier,
            IdentityType = response.Identity?.Type,
            Identity = response.Identity,
            KubernetesVersion = props?.KubernetesVersion,
            ProvisioningState = props?.ProvisioningState,
            DnsPrefix = props?.DnsPrefix,
            Fqdn = props?.Fqdn,
            NodeResourceGroup = props?.NodeResourceGroup,
            SupportPlan = props?.SupportPlan,
            ResourceUid = props?.ResourceUid,
            EnableRbac = props?.EnableRbac,
            DisableLocalAccounts = props?.DisableLocalAccounts,
            MaxAgentPools = props?.MaxAgentPools,
            PowerState = props?.PowerState?.Code,
            NetworkPlugin = np?.NetworkPlugin,
            NetworkPolicy = np?.NetworkPolicy,
            ServiceCidr = np?.ServiceCidr,
            DnsServiceIP = np?.DnsServiceIP,
            NetworkProfile = MapToNetworkProfile(np),
            OidcIssuerProfile = props?.OidcIssuerProfile,
            AutoUpgradeProfile = props?.AutoUpgradeProfile,
            SecurityProfile = props?.SecurityProfile,
            StorageProfile = props?.StorageProfile,
            WorkloadAutoScalerProfile = props?.WorkloadAutoScalerProfile,
            AddonProfiles = MapAddonProfiles(props?.AddonProfiles),
            IdentityProfile = props?.IdentityProfile,
            AgentPoolProfiles = props?.AgentPoolProfiles,
            NodeCount = props?.AgentPoolProfiles?.Count > 0 ? props.AgentPoolProfiles[0].Count : null,
            NodeVmSize = props?.AgentPoolProfiles?.Count > 0 ? props.AgentPoolProfiles[0].VmSize : null,
        };

        return cluster;
    }

    private static ClusterNetworkProfile? MapToNetworkProfile(AksClusterNetworkProfileJson? np)
    {
        if (np is null)
            return null;

        ClusterNetworkLoadBalancerProfile? lbProfile = null;
        if (np.LoadBalancerProfile is { } lb)
        {
            lbProfile = new()
            {
                ManagedOutboundIPCount = lb.ManagedOutboundIPs?.Count,
                EffectiveOutboundIPs = lb.EffectiveOutboundIPs,
                BackendPoolType = lb.BackendPoolType,
            };
        }

        return new()
        {
            NetworkPlugin = np.NetworkPlugin,
            NetworkPluginMode = np.NetworkPluginMode,
            NetworkPolicy = np.NetworkPolicy,
            NetworkDataplane = np.NetworkDataplane,
            LoadBalancerSku = np.LoadBalancerSku,
            LoadBalancerProfile = lbProfile,
            PodCidr = np.PodCidr,
            ServiceCidr = np.ServiceCidr,
            DnsServiceIP = np.DnsServiceIP,
            OutboundType = np.OutboundType,
            PodCidrs = np.PodCidrs,
            ServiceCidrs = np.ServiceCidrs,
            IpFamilies = np.IpFamilies,
        };
    }

    private static IDictionary<string, IDictionary<string, string>>? MapAddonProfiles(
        Dictionary<string, AksAddonProfileJson>? addonProfiles)
    {
        if (addonProfiles is null)
            return null;

        var result = new Dictionary<string, IDictionary<string, string>>();
        foreach (var (name, addon) in addonProfiles)
        {
            var map = new Dictionary<string, string>();
            if (addon.Config is not null)
            {
                foreach (var (key, value) in addon.Config)
                    map[$"config.{key}"] = value;
            }
            if (addon.Identity is not null)
            {
                if (addon.Identity.ClientId is not null)
                    map["identity.clientId"] = addon.Identity.ClientId;
                if (addon.Identity.ObjectId is not null)
                    map["identity.objectId"] = addon.Identity.ObjectId;
            }
            result[name] = map;
        }

        return result.Count > 0 ? result : null;
    }
    private static NodePool ConvertToNodePoolModel(ContainerServiceAgentPoolResource agentPoolResource)
    {
        var data = agentPoolResource.Data;

        return new()
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
            PowerState = data.PowerStateCode.HasValue ? new() { Code = data.PowerStateCode.Value.ToString() } : null,
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
            NodeImageVersion = data.NodeImageVersion,
            Tags = data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            SpotMaxPrice = data.SpotMaxPrice,
            WorkloadRuntime = data.WorkloadRuntime?.ToString(),
            EnableEncryptionAtHost = data.EnableEncryptionAtHost,
            UpgradeSettings = data.UpgradeSettings is null ? null : new()
            {
                MaxSurge = data.UpgradeSettings.MaxSurge,
                MaxUnavailable = null
            },
            NetworkProfile = data.NetworkProfile is null ? null : new()
            {
                AllowedHostPorts = data.NetworkProfile.AllowedHostPorts?.Select(p => new PortRange { StartPort = p.PortStart, EndPort = p.PortEnd }).ToList(),
                ApplicationSecurityGroups = data.NetworkProfile.ApplicationSecurityGroups?.Select(rid => rid.ToString()).ToList(),
                NodePublicIPTags = data.NetworkProfile.NodePublicIPTags?.Select(t => new IPTag { IpTagType = t.IPTagType, Tag = t.Tag }).ToList()
            },
            PodSubnetId = data.PodSubnetId,
            VnetSubnetId = data.VnetSubnetId
        };
    }

    private static NodePool ConvertToNodePoolModel(ManagedClusterAgentPoolProfile profile)
    {
        return new()
        {
            Name = profile.Name,
            Count = profile.Count,
            VmSize = profile.VmSize?.ToString(),
            OsDiskSizeGB = profile.OSDiskSizeInGB,
            OsDiskType = profile.OSDiskType?.ToString(),
            KubeletDiskType = profile.KubeletDiskType?.ToString(),
            MaxPods = profile.MaxPods,
            Type = profile.AgentPoolType?.ToString(),
            MaxCount = profile.MaxCount,
            MinCount = profile.MinCount,
            EnableAutoScaling = profile.EnableAutoScaling,
            ScaleDownMode = profile.ScaleDownMode?.ToString(),
            ProvisioningState = profile.ProvisioningState?.ToString(),
            PowerState = profile.PowerStateCode.HasValue ? new() { Code = profile.PowerStateCode.Value.ToString() } : null,
            Mode = profile.Mode?.ToString(),
            OrchestratorVersion = profile.OrchestratorVersion,
            CurrentOrchestratorVersion = profile.CurrentOrchestratorVersion,
            EnableNodePublicIP = profile.EnableNodePublicIP,
            ScaleSetPriority = profile.ScaleSetPriority?.ToString(),
            ScaleSetEvictionPolicy = profile.ScaleSetEvictionPolicy?.ToString(),
            NodeLabels = profile.NodeLabels?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            NodeTaints = profile.NodeTaints?.ToList(),
            OsType = profile.OSType?.ToString(),
            OsSKU = profile.OSSku?.ToString(),
            NodeImageVersion = profile.NodeImageVersion,
            Tags = profile.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
            SpotMaxPrice = profile.SpotMaxPrice,
            WorkloadRuntime = profile.WorkloadRuntime?.ToString(),
            EnableEncryptionAtHost = profile.EnableEncryptionAtHost,
            EnableUltraSSD = profile.EnableUltraSsd,
            EnableFIPS = profile.EnableFips,
            // Profiles don't expose GPU/Security sub-objects in this API shape
            NetworkProfile = profile.NetworkProfile is null ? null : new()
            {
                AllowedHostPorts = profile.NetworkProfile.AllowedHostPorts?.Select(p => new PortRange { StartPort = p.PortStart, EndPort = p.PortEnd }).ToList(),
                ApplicationSecurityGroups = profile.NetworkProfile.ApplicationSecurityGroups?.Select(rid => rid.ToString()).ToList(),
                NodePublicIPTags = profile.NetworkProfile.NodePublicIPTags?.Select(t => new IPTag { IpTagType = t.IPTagType, Tag = t.Tag }).ToList()
            },
            PodSubnetId = profile.PodSubnetId?.ToString(),
            VnetSubnetId = profile.VnetSubnetId?.ToString()
        };
    }
}
