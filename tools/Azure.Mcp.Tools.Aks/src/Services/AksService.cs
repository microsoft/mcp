// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Core.Services.Caching;
using Azure.Mcp.Tools.Aks.Models;
using Azure.ResourceManager.ContainerService;
using Azure.ResourceManager.ContainerService.Models;

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

    public async Task<List<Cluster>> GetClusters(
        string subscription,
        string? clusterName,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        if (string.IsNullOrEmpty(clusterName))
        {
            // Create cache key
            var cacheKey = $"{AksClustersCacheKey}_{subscription}";
            if (!string.IsNullOrEmpty(resourceGroup))
            {
                cacheKey += $"_{resourceGroup}";
            }
            if (!string.IsNullOrEmpty(tenant))
            {
                cacheKey += $"_{tenant}";
            }

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
                if (string.IsNullOrEmpty(resourceGroup))
                {

                    await foreach (var cluster in subscriptionResource.GetContainerServiceManagedClustersAsync())
                    {
                        if (cluster?.Data != null)
                        {
                            clusters.Add(ConvertToClusterModel(cluster));
                        }
                    }


                }
                else
                {
                    var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
                    if (resourceGroupResource?.Value == null)
                    {
                        return clusters;
                    }

                    await foreach (var cluster in resourceGroupResource.Value.GetContainerServiceManagedClusters().GetAllAsync())
                    {
                        if (cluster?.Data != null)
                        {
                            clusters.Add(ConvertToClusterModel(cluster));
                        }
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
        else
        {
            ValidateRequiredParameters((nameof(resourceGroup), resourceGroup), (nameof(clusterName), clusterName));

            // Create cache key
            var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"cluster_{subscription}_{resourceGroup}_{clusterName}"
            : $"cluster_{subscription}_{resourceGroup}_{clusterName}_{tenant}";

            // Try to get from cache first
            var cachedCluster = await _cacheService.GetAsync<List<Cluster>>(CacheGroup, cacheKey, s_cacheDuration);
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
                    return [];
                }

                var clusterResource = await resourceGroupResource.Value
                    .GetContainerServiceManagedClusters()
                    .GetAsync(clusterName);

                if (clusterResource?.Value?.Data == null)
                {
                    return [];
                }

                var clusters = new List<Cluster>() { ConvertToClusterModel(clusterResource.Value) };

                // Cache the result
                await _cacheService.SetAsync(CacheGroup, cacheKey, clusters, s_cacheDuration);

                return clusters;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving AKS cluster '{clusterName}': {ex.Message}", ex);
            }
        }
    }

    public async Task<List<NodePool>> GetNodePools(
        string subscription,
        string resourceGroup,
        string clusterName,
        string? nodePoolName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(clusterName), clusterName));

        if (string.IsNullOrEmpty(nodePoolName))
        {
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
        else
        {
            // Create cache key
            var cacheKey = string.IsNullOrEmpty(tenant)
            ? $"nodepool_{subscription}_{resourceGroup}_{clusterName}_{nodePoolName}"
            : $"nodepool_{subscription}_{resourceGroup}_{clusterName}_{nodePoolName}_{tenant}";

            // Try to get from cache first
            var cachedNodePool = await _cacheService.GetAsync<List<NodePool>>(CacheGroup, cacheKey, s_cacheDuration);
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
                    return [];
                }

                var clusterResource = await resourceGroupResource.Value
                    .GetContainerServiceManagedClusters()
                    .GetAsync(clusterName);

                if (clusterResource?.Value == null)
                {
                    return [];
                }

                var agentPoolResource = await clusterResource.Value
                    .GetContainerServiceAgentPools()
                    .GetAsync(nodePoolName);

                if (agentPoolResource?.Value?.Data == null)
                {
                    return [];
                }

                var nodePools = new List<NodePool>() { ConvertToNodePoolModel(agentPoolResource.Value) };

                // Cache the result
                await _cacheService.SetAsync(CacheGroup, cacheKey, nodePools, s_cacheDuration);

                return nodePools;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving AKS node pool '{nodePoolName}' for cluster '{clusterName}': {ex.Message}", ex);
            }
        }
    }

    private static Cluster ConvertToClusterModel(ContainerServiceManagedClusterResource clusterResource)
    {
        var data = clusterResource.Data;
        var agentPool = data.AgentPoolProfiles?.FirstOrDefault();

        var cluster = new Cluster
        {
            Id = clusterResource.Id.ToString(),
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
            Identity = new()
            {
                Type = data.Identity?.ManagedServiceIdentityType.ToString(),
                PrincipalId = data.Identity?.PrincipalId?.ToString(),
                TenantId = data.Identity?.TenantId?.ToString()
            },
            EnableRbac = data.EnableRbac,
            NetworkPlugin = data.NetworkProfile?.NetworkPlugin?.ToString(),
            NetworkPolicy = data.NetworkProfile?.NetworkPolicy?.ToString(),
            ServiceCidr = data.NetworkProfile?.ServiceCidr,
            DnsServiceIP = data.NetworkProfile?.DnsServiceIP?.ToString(),
            SkuTier = data.Sku?.Tier?.ToString(),
            SkuName = data.Sku?.Name?.ToString(),
            NodeResourceGroup = data.NodeResourceGroup,
            MaxAgentPools = data.MaxAgentPools,
            SupportPlan = data.SupportPlan?.ToString(),
            NetworkProfile = new()
            {
                NetworkPlugin = data.NetworkProfile?.NetworkPlugin?.ToString(),
                NetworkPluginMode = data.NetworkProfile?.NetworkPluginMode?.ToString(),
                NetworkPolicy = data.NetworkProfile?.NetworkPolicy?.ToString(),
                NetworkDataplane = data.NetworkProfile?.NetworkDataplane?.ToString(),
                LoadBalancerSku = data.NetworkProfile?.LoadBalancerSku?.ToString(),
                LoadBalancerProfile = data.NetworkProfile?.LoadBalancerProfile is null ? null : new()
                {
                    ManagedOutboundIPCount = data.NetworkProfile?.LoadBalancerProfile?.ManagedOutboundIPs?.Count,
                    EffectiveOutboundIPs = data.NetworkProfile?.LoadBalancerProfile?.EffectiveOutboundIPs?.Select(e => new EffectiveOutboundIPReference() { Id = e.Id?.ToString() }).ToList(),
                    BackendPoolType = data.NetworkProfile?.LoadBalancerProfile?.BackendPoolType?.ToString()
                },
                PodCidr = data.NetworkProfile?.PodCidr,
                ServiceCidr = data.NetworkProfile?.ServiceCidr,
                DnsServiceIP = data.NetworkProfile?.DnsServiceIP?.ToString(),
                OutboundType = data.NetworkProfile?.OutboundType?.ToString(),
                PodCidrs = data.NetworkProfile?.PodCidrs?.ToList(),
                ServiceCidrs = data.NetworkProfile?.ServiceCidrs?.ToList(),
                IpFamilies = data.NetworkProfile?.IPFamilies?.Select(f => f.ToString()).ToList()
            },
            WindowsProfile = data.WindowsProfile is null ? null : new() { AdminUsername = data.WindowsProfile.AdminUsername },
            ServicePrincipalProfile = data.ServicePrincipalProfile is null ? null : new() { ClientId = data.ServicePrincipalProfile.ClientId },
            AutoUpgradeProfile = data.AutoUpgradeProfile is null ? null : new()
            {
                UpgradeChannel = data.AutoUpgradeProfile.UpgradeChannel?.ToString(),
                NodeOSUpgradeChannel = data.AutoUpgradeProfile.NodeOSUpgradeChannel?.ToString()
            },
            // OIDC Issuer Profile
            OidcIssuerProfile = data.OidcIssuerProfile is null ? null : new()
            {
                Enabled = data.OidcIssuerProfile.IsEnabled,
                IssuerUrl = data.OidcIssuerProfile.IssuerUriInfo
            },
            AddonProfiles = data.AddonProfiles?.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    IDictionary<string, string> map = new Dictionary<string, string>();
                    if (kvp.Value != null)
                    {
                        if (kvp.Value.Config != null)
                        {
                            foreach (var c in kvp.Value.Config)
                            {
                                map[$"config.{c.Key}"] = c.Value;
                            }
                        }
                        if (kvp.Value.Identity != null)
                        {
                            if (kvp.Value.Identity.ClientId != null)
                                map.Add("identity.clientId", kvp.Value.Identity.ClientId.ToString()!);
                            if (kvp.Value.Identity.ObjectId != null)
                                map.Add("identity.objectId", kvp.Value.Identity.ObjectId.ToString()!);
                        }
                    }
                    return map;
                }),
            IdentityProfile = data.IdentityProfile?.ToDictionary(
                kvp => kvp.Key,
                kvp => new ManagedIdentityReference
                {
                    ResourceId = kvp.Value?.ResourceId?.ToString(),
                    ClientId = kvp.Value?.ClientId?.ToString(),
                    ObjectId = kvp.Value?.ObjectId?.ToString()
                }),
            DisableLocalAccounts = data.DisableLocalAccounts,
            // Security Profile
            SecurityProfile = data.SecurityProfile is null ? null : new()
            {
                AzureKeyVaultKms = data.SecurityProfile.AzureKeyVaultKms is null ? null : new()
                {
                    Enabled = data.SecurityProfile.AzureKeyVaultKms.IsEnabled,
                    KeyId = data.SecurityProfile.AzureKeyVaultKms.KeyId?.ToString()
                },
                Defender = data.SecurityProfile.Defender is null ? null : new()
                {
                    LogAnalyticsWorkspaceResourceId = data.SecurityProfile.Defender.LogAnalyticsWorkspaceResourceId?.ToString(),
                    SecurityMonitoring = new() { Enabled = data.SecurityProfile.Defender.IsSecurityMonitoringEnabled }
                },
                ImageCleaner = data.SecurityProfile.ImageCleaner is null ? null : new()
                {
                    Enabled = data.SecurityProfile.ImageCleaner.IsEnabled,
                    IntervalHours = data.SecurityProfile.ImageCleaner.IntervalHours
                },
                WorkloadIdentity = data.SecurityProfile.IsWorkloadIdentityEnabled is null
                    ? null
                    : new() { Enabled = data.SecurityProfile.IsWorkloadIdentityEnabled }
            },
            // Storage Profile
            StorageProfile = data.StorageProfile is null ? null : new()
            {
                BlobCSIDriver = data.StorageProfile.IsBlobCsiDriverEnabled is null ? null : new() { Enabled = data.StorageProfile.IsBlobCsiDriverEnabled },
                DiskCSIDriver = data.StorageProfile.IsDiskCsiDriverEnabled is null ? null : new() { Enabled = data.StorageProfile.IsDiskCsiDriverEnabled },
                FileCSIDriver = data.StorageProfile.IsFileCsiDriverEnabled is null ? null : new() { Enabled = data.StorageProfile.IsFileCsiDriverEnabled },
                SnapshotController = data.StorageProfile.IsSnapshotControllerEnabled is null ? null : new() { Enabled = data.StorageProfile.IsSnapshotControllerEnabled }
            },
            // Metrics profile (no 1.2.5 SDK match for our CostAnalysis model)
            MetricsProfile = null,
            // Node provisioning and bootstrap profiles are not exposed in SDK 1.2.5
            NodeProvisioningProfile = null,
            BootstrapProfile = null,
            // Workload Auto-scaler profile
            WorkloadAutoScalerProfile = data.WorkloadAutoScalerProfile is null ? null : new()
            {
                Keda = data.WorkloadAutoScalerProfile.IsKedaEnabled is null ? null : new() { Enabled = data.WorkloadAutoScalerProfile.IsKedaEnabled },
                VerticalPodAutoscaler = data.WorkloadAutoScalerProfile.IsVpaEnabled is null ? null : new() { Enabled = data.WorkloadAutoScalerProfile.IsVpaEnabled }
            },
            // AI toolchain operator profile not exposed in SDK 1.2.5
            AiToolchainOperatorProfile = null,
            // Unique resource UID
            ResourceUid = data.ResourceId,
            Tags = data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        };

        // Map agent pool profiles from the cluster resource data when available
        if (data.AgentPoolProfiles is not null)
        {
            try
            {
                cluster.AgentPoolProfiles = data.AgentPoolProfiles.Select(ConvertToNodePoolModel).ToList();
            }
            catch
            {
                // If SDK shape differs, fall back to minimal projection
                cluster.AgentPoolProfiles = data.AgentPoolProfiles
                    .Select(p => new NodePool { Name = p.Name, Count = p.Count, VmSize = p.VmSize?.ToString(), Mode = p.Mode?.ToString() })
                    .ToList();
            }
        }

        return cluster;
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
        return new NodePool
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
