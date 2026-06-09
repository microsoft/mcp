// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
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
            ValidateRequiredParameters((nameof(resourceGroup), resourceGroup), (nameof(clusterName), clusterName));

            // Create cache key
            var cacheKey = string.IsNullOrEmpty(tenant)
                ? CacheKeyBuilder.Build("cluster", subscription, resourceGroup!, clusterName)
                : CacheKeyBuilder.Build("cluster", subscription, resourceGroup!, clusterName, tenant);

            // Try to get from cache first
            var cachedCluster = await _cacheService.GetAsync<List<Cluster>>(CacheGroup, cacheKey, s_cacheDuration, cancellationToken);
            if (cachedCluster != null)
            {
                return cachedCluster;
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

            if (clusterResource?.Value?.Data == null)
            {
                return [];
            }

            var clusters = new List<Cluster>() { ConvertToClusterModel(clusterResource.Value) };

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
        var cluster = new Cluster
        {
            Id = item.TryGetProperty("id", out var idEl) ? idEl.GetString() : null,
            Name = item.TryGetProperty("name", out var nameEl) ? nameEl.GetString() : null,
            SubscriptionId = item.TryGetProperty("subscriptionId", out var subEl) ? subEl.GetString() : null,
            ResourceGroupName = item.TryGetProperty("resourceGroup", out var rgEl) ? rgEl.GetString() : null,
            Location = item.TryGetProperty("location", out var locEl) ? locEl.GetString() : null,
            Tags = ReadStringDictionaryFromProperty(item, "tags"),
        };

        if (item.TryGetProperty("sku", out var skuEl))
        {
            cluster.SkuName = skuEl.TryGetProperty("name", out var skuNameEl) ? skuNameEl.GetString() : null;
            cluster.SkuTier = skuEl.TryGetProperty("tier", out var skuTierEl) ? skuTierEl.GetString() : null;
        }

        if (item.TryGetProperty("identity", out var identEl))
        {
            cluster.IdentityType = identEl.TryGetProperty("type", out var itEl) ? itEl.GetString() : null;
            cluster.Identity = new()
            {
                Type = cluster.IdentityType,
                PrincipalId = identEl.TryGetProperty("principalId", out var pidEl) ? pidEl.GetString() : null,
                TenantId = identEl.TryGetProperty("tenantId", out var tidEl) ? tidEl.GetString() : null,
            };
        }

        if (!item.TryGetProperty("properties", out var props))
            return cluster;

        cluster.KubernetesVersion = props.TryGetProperty("kubernetesVersion", out var kvEl) ? kvEl.GetString() : null;
        cluster.ProvisioningState = props.TryGetProperty("provisioningState", out var psEl) ? psEl.GetString() : null;
        cluster.DnsPrefix = props.TryGetProperty("dnsPrefix", out var dpEl) ? dpEl.GetString() : null;
        cluster.Fqdn = props.TryGetProperty("fqdn", out var fqdnEl) ? fqdnEl.GetString() : null;
        cluster.NodeResourceGroup = props.TryGetProperty("nodeResourceGroup", out var nrgEl) ? nrgEl.GetString() : null;
        cluster.SupportPlan = props.TryGetProperty("supportPlan", out var spEl) ? spEl.GetString() : null;
        cluster.ResourceUid = props.TryGetProperty("resourceUID", out var ruidEl) ? ruidEl.GetString() : null;
        cluster.EnableRbac = TryGetNullableBool(props, "enableRBAC");
        cluster.DisableLocalAccounts = TryGetNullableBool(props, "disableLocalAccounts");

        if (props.TryGetProperty("maxAgentPools", out var mapEl) && mapEl.TryGetInt32(out var mapVal))
            cluster.MaxAgentPools = mapVal;

        if (props.TryGetProperty("powerState", out var psStateEl))
            cluster.PowerState = psStateEl.TryGetProperty("code", out var psCodeEl) ? psCodeEl.GetString() : null;

        if (props.TryGetProperty("networkProfile", out var npEl))
        {
            cluster.NetworkPlugin = npEl.TryGetProperty("networkPlugin", out var npluginEl) ? npluginEl.GetString() : null;
            cluster.NetworkPolicy = npEl.TryGetProperty("networkPolicy", out var npolicyEl) ? npolicyEl.GetString() : null;
            cluster.ServiceCidr = npEl.TryGetProperty("serviceCidr", out var scCidrEl) ? scCidrEl.GetString() : null;
            cluster.DnsServiceIP = npEl.TryGetProperty("dnsServiceIP", out var dnsEl) ? dnsEl.GetString() : null;
            cluster.NetworkProfile = ReadNetworkProfile(npEl);
        }

        if (props.TryGetProperty("oidcIssuerProfile", out var oidcEl))
        {
            cluster.OidcIssuerProfile = new()
            {
                Enabled = TryGetNullableBool(oidcEl, "enabled"),
                IssuerUrl = oidcEl.TryGetProperty("issuerURL", out var iuEl) ? iuEl.GetString() : null,
            };
        }

        if (props.TryGetProperty("autoUpgradeProfile", out var aupEl))
        {
            cluster.AutoUpgradeProfile = new()
            {
                UpgradeChannel = aupEl.TryGetProperty("upgradeChannel", out var ucEl) ? ucEl.GetString() : null,
                NodeOSUpgradeChannel = aupEl.TryGetProperty("nodeOSUpgradeChannel", out var noscEl) ? noscEl.GetString() : null,
            };
        }

        if (props.TryGetProperty("securityProfile", out var secEl))
            cluster.SecurityProfile = ReadSecurityProfile(secEl);

        if (props.TryGetProperty("storageProfile", out var stEl))
            cluster.StorageProfile = ReadStorageProfile(stEl);

        if (props.TryGetProperty("workloadAutoScalerProfile", out var waspEl))
        {
            cluster.WorkloadAutoScalerProfile = new()
            {
                Keda = waspEl.TryGetProperty("keda", out var kedaEl)
                    ? new() { Enabled = TryGetNullableBool(kedaEl, "enabled") }
                    : null,
                VerticalPodAutoscaler = waspEl.TryGetProperty("verticalPodAutoscaler", out var vpaEl)
                    ? new() { Enabled = TryGetNullableBool(vpaEl, "enabled") }
                    : null,
            };
        }

        if (props.TryGetProperty("addonProfiles", out var addonEl) && addonEl.ValueKind == JsonValueKind.Object)
        {
            var addonProfiles = new Dictionary<string, IDictionary<string, string>>();
            foreach (var addon in addonEl.EnumerateObject())
            {
                var map = new Dictionary<string, string>();
                if (addon.Value.TryGetProperty("config", out var configEl) && configEl.ValueKind == JsonValueKind.Object)
                {
                    foreach (var cfg in configEl.EnumerateObject())
                        map[$"config.{cfg.Name}"] = cfg.Value.GetString() ?? string.Empty;
                }
                if (addon.Value.TryGetProperty("identity", out var addonIdentEl))
                {
                    if (addonIdentEl.TryGetProperty("clientId", out var cliEl))
                        map["identity.clientId"] = cliEl.GetString() ?? string.Empty;
                    if (addonIdentEl.TryGetProperty("objectId", out var objEl))
                        map["identity.objectId"] = objEl.GetString() ?? string.Empty;
                }
                addonProfiles[addon.Name] = map;
            }
            cluster.AddonProfiles = addonProfiles;
        }

        if (props.TryGetProperty("identityProfile", out var ipEl) && ipEl.ValueKind == JsonValueKind.Object)
        {
            var identityProfile = new Dictionary<string, ManagedIdentityReference>();
            foreach (var entry in ipEl.EnumerateObject())
            {
                identityProfile[entry.Name] = new()
                {
                    ResourceId = entry.Value.TryGetProperty("resourceId", out var rIdEl) ? rIdEl.GetString() : null,
                    ClientId = entry.Value.TryGetProperty("clientId", out var cIdEl) ? cIdEl.GetString() : null,
                    ObjectId = entry.Value.TryGetProperty("objectId", out var oIdEl) ? oIdEl.GetString() : null,
                };
            }
            cluster.IdentityProfile = identityProfile;
        }

        if (props.TryGetProperty("agentPoolProfiles", out var appEl) && appEl.ValueKind == JsonValueKind.Array)
        {
            var pools = new List<NodePool>();
            foreach (var poolEl in appEl.EnumerateArray())
                pools.Add(ReadNodePoolFromJson(poolEl));
            cluster.AgentPoolProfiles = pools;

            var first = pools.Count > 0 ? pools[0] : null;
            cluster.NodeCount = first?.Count;
            cluster.NodeVmSize = first?.VmSize;
        }

        return cluster;
    }

    private static bool? TryGetNullableBool(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var el))
            return null;
        return el.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null,
        };
    }

    private static IDictionary<string, string>? ReadStringDictionaryFromProperty(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var dictEl) || dictEl.ValueKind != JsonValueKind.Object)
            return null;
        var result = new Dictionary<string, string>();
        foreach (var prop in dictEl.EnumerateObject())
            result[prop.Name] = prop.Value.GetString() ?? string.Empty;
        return result.Count > 0 ? result : null;
    }

    private static ClusterNetworkProfile ReadNetworkProfile(JsonElement npEl)
    {
        ClusterNetworkLoadBalancerProfile? lbProfile = null;
        if (npEl.TryGetProperty("loadBalancerProfile", out var lbEl))
        {
            int? managedOutboundIPCount = null;
            if (lbEl.TryGetProperty("managedOutboundIPs", out var moEl)
                && moEl.TryGetProperty("count", out var moCountEl)
                && moCountEl.TryGetInt32(out var moCountVal))
            {
                managedOutboundIPCount = moCountVal;
            }

            List<EffectiveOutboundIPReference>? effectiveOutboundIPs = null;
            if (lbEl.TryGetProperty("effectiveOutboundIPs", out var eoEl) && eoEl.ValueKind == JsonValueKind.Array)
            {
                effectiveOutboundIPs = [];
                foreach (var eoItem in eoEl.EnumerateArray())
                    effectiveOutboundIPs.Add(new() { Id = eoItem.TryGetProperty("id", out var eoIdEl) ? eoIdEl.GetString() : null });
            }

            lbProfile = new()
            {
                ManagedOutboundIPCount = managedOutboundIPCount,
                EffectiveOutboundIPs = effectiveOutboundIPs,
                BackendPoolType = lbEl.TryGetProperty("backendPoolType", out var bptEl) ? bptEl.GetString() : null,
            };
        }

        List<string>? podCidrs = null;
        if (npEl.TryGetProperty("podCidrs", out var podCidrsEl) && podCidrsEl.ValueKind == JsonValueKind.Array)
            podCidrs = podCidrsEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList();

        List<string>? serviceCidrs = null;
        if (npEl.TryGetProperty("serviceCidrs", out var svcCidrsEl) && svcCidrsEl.ValueKind == JsonValueKind.Array)
            serviceCidrs = svcCidrsEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList();

        List<string>? ipFamilies = null;
        if (npEl.TryGetProperty("ipFamilies", out var ipFamsEl) && ipFamsEl.ValueKind == JsonValueKind.Array)
            ipFamilies = ipFamsEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList();

        return new()
        {
            NetworkPlugin = npEl.TryGetProperty("networkPlugin", out var npluginEl) ? npluginEl.GetString() : null,
            NetworkPluginMode = npEl.TryGetProperty("networkPluginMode", out var npmodeEl) ? npmodeEl.GetString() : null,
            NetworkPolicy = npEl.TryGetProperty("networkPolicy", out var npolicyEl) ? npolicyEl.GetString() : null,
            NetworkDataplane = npEl.TryGetProperty("networkDataplane", out var ndpEl) ? ndpEl.GetString() : null,
            LoadBalancerSku = npEl.TryGetProperty("loadBalancerSku", out var lbSkuEl) ? lbSkuEl.GetString() : null,
            LoadBalancerProfile = lbProfile,
            PodCidr = npEl.TryGetProperty("podCidr", out var pcCidrEl) ? pcCidrEl.GetString() : null,
            ServiceCidr = npEl.TryGetProperty("serviceCidr", out var scEl) ? scEl.GetString() : null,
            DnsServiceIP = npEl.TryGetProperty("dnsServiceIP", out var dnsEl2) ? dnsEl2.GetString() : null,
            OutboundType = npEl.TryGetProperty("outboundType", out var otEl) ? otEl.GetString() : null,
            PodCidrs = podCidrs,
            ServiceCidrs = serviceCidrs,
            IpFamilies = ipFamilies,
        };
    }

    private static ClusterSecurityProfile ReadSecurityProfile(JsonElement secEl)
    {
        AzureKeyVaultKms? kms = null;
        if (secEl.TryGetProperty("azureKeyVaultKms", out var kmsEl))
        {
            kms = new()
            {
                Enabled = TryGetNullableBool(kmsEl, "enabled"),
                KeyId = kmsEl.TryGetProperty("keyId", out var kidEl) ? kidEl.GetString() : null,
            };
        }

        DefenderProfile? defender = null;
        if (secEl.TryGetProperty("defender", out var defEl))
        {
            DefenderSecurityMonitoring? monitoring = null;
            if (defEl.TryGetProperty("securityMonitoring", out var smEl))
                monitoring = new() { Enabled = TryGetNullableBool(smEl, "enabled") };

            defender = new()
            {
                LogAnalyticsWorkspaceResourceId = defEl.TryGetProperty("logAnalyticsWorkspaceResourceId", out var lawEl) ? lawEl.GetString() : null,
                SecurityMonitoring = monitoring,
            };
        }

        ImageCleanerProfile? imageCleaner = null;
        if (secEl.TryGetProperty("imageCleaner", out var icEl))
        {
            int? intervalHours = null;
            if (icEl.TryGetProperty("intervalHours", out var ihEl) && ihEl.TryGetInt32(out var ihVal))
                intervalHours = ihVal;
            imageCleaner = new()
            {
                Enabled = TryGetNullableBool(icEl, "enabled"),
                IntervalHours = intervalHours,
            };
        }

        WorkloadIdentityProfile? workloadIdentity = null;
        if (secEl.TryGetProperty("workloadIdentity", out var wiEl))
            workloadIdentity = new() { Enabled = TryGetNullableBool(wiEl, "enabled") };

        return new()
        {
            AzureKeyVaultKms = kms,
            Defender = defender,
            ImageCleaner = imageCleaner,
            WorkloadIdentity = workloadIdentity,
        };
    }

    private static ClusterStorageProfile ReadStorageProfile(JsonElement stEl)
    {
        return new()
        {
            BlobCSIDriver = stEl.TryGetProperty("blobCSIDriver", out var blobEl) ? new() { Enabled = TryGetNullableBool(blobEl, "enabled") } : null,
            DiskCSIDriver = stEl.TryGetProperty("diskCSIDriver", out var diskEl) ? new() { Enabled = TryGetNullableBool(diskEl, "enabled") } : null,
            FileCSIDriver = stEl.TryGetProperty("fileCSIDriver", out var fileEl) ? new() { Enabled = TryGetNullableBool(fileEl, "enabled") } : null,
            SnapshotController = stEl.TryGetProperty("snapshotController", out var scEl) ? new() { Enabled = TryGetNullableBool(scEl, "enabled") } : null,
        };
    }

    private static NodePool ReadNodePoolFromJson(JsonElement poolEl)
    {
        NodePoolPowerState? powerState = null;
        if (poolEl.TryGetProperty("powerState", out var psEl))
            powerState = new() { Code = psEl.TryGetProperty("code", out var psCodeEl) ? psCodeEl.GetString() : null };

        NodePoolUpgradeSettings? upgradeSettings = null;
        if (poolEl.TryGetProperty("upgradeSettings", out var usEl))
        {
            upgradeSettings = new()
            {
                MaxSurge = usEl.TryGetProperty("maxSurge", out var msEl) ? msEl.GetString() : null,
                MaxUnavailable = usEl.TryGetProperty("maxUnavailable", out var muEl) ? muEl.GetString() : null,
            };
        }

        NodePoolNetworkProfile? networkProfile = null;
        if (poolEl.TryGetProperty("networkProfile", out var npEl))
        {
            List<PortRange>? allowedHostPorts = null;
            if (npEl.TryGetProperty("allowedHostPorts", out var ahpEl) && ahpEl.ValueKind == JsonValueKind.Array)
            {
                allowedHostPorts = [];
                foreach (var portEl in ahpEl.EnumerateArray())
                {
                    int? startPort = null, endPort = null;
                    if (portEl.TryGetProperty("portStart", out var spEl) && spEl.TryGetInt32(out var spVal)) startPort = spVal;
                    if (portEl.TryGetProperty("portEnd", out var epEl) && epEl.TryGetInt32(out var epVal)) endPort = epVal;
                    allowedHostPorts.Add(new() { StartPort = startPort, EndPort = endPort });
                }
            }

            List<string>? applicationSecurityGroups = null;
            if (npEl.TryGetProperty("applicationSecurityGroups", out var asgEl) && asgEl.ValueKind == JsonValueKind.Array)
                applicationSecurityGroups = asgEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList();

            List<IPTag>? nodePublicIPTags = null;
            if (npEl.TryGetProperty("nodePublicIPTags", out var ipTagsEl) && ipTagsEl.ValueKind == JsonValueKind.Array)
            {
                nodePublicIPTags = [];
                foreach (var tagEl in ipTagsEl.EnumerateArray())
                {
                    nodePublicIPTags.Add(new()
                    {
                        IpTagType = tagEl.TryGetProperty("ipTagType", out var ittEl) ? ittEl.GetString() : null,
                        Tag = tagEl.TryGetProperty("tag", out var tagValEl) ? tagValEl.GetString() : null,
                    });
                }
            }

            networkProfile = new()
            {
                AllowedHostPorts = allowedHostPorts,
                ApplicationSecurityGroups = applicationSecurityGroups,
                NodePublicIPTags = nodePublicIPTags,
            };
        }

        IList<string>? nodeTaints = null;
        if (poolEl.TryGetProperty("nodeTaints", out var ntEl) && ntEl.ValueKind == JsonValueKind.Array)
            nodeTaints = ntEl.EnumerateArray().Select(e => e.GetString() ?? string.Empty).ToList();

        return new()
        {
            Name = poolEl.TryGetProperty("name", out var poolNameEl) ? poolNameEl.GetString() : null,
            Count = poolEl.TryGetProperty("count", out var countEl) && countEl.TryGetInt32(out var countVal) ? countVal : null,
            VmSize = poolEl.TryGetProperty("vmSize", out var vmSizeEl) ? vmSizeEl.GetString() : null,
            OsDiskSizeGB = poolEl.TryGetProperty("osDiskSizeGB", out var odsEl) && odsEl.TryGetInt32(out var odsVal) ? odsVal : null,
            OsDiskType = poolEl.TryGetProperty("osDiskType", out var odtEl) ? odtEl.GetString() : null,
            KubeletDiskType = poolEl.TryGetProperty("kubeletDiskType", out var kdtEl) ? kdtEl.GetString() : null,
            MaxPods = poolEl.TryGetProperty("maxPods", out var mpEl) && mpEl.TryGetInt32(out var mpVal) ? mpVal : null,
            Type = poolEl.TryGetProperty("type", out var typeEl) ? typeEl.GetString() : null,
            MaxCount = poolEl.TryGetProperty("maxCount", out var maxCEl) && maxCEl.TryGetInt32(out var maxCVal) ? maxCVal : null,
            MinCount = poolEl.TryGetProperty("minCount", out var minCEl) && minCEl.TryGetInt32(out var minCVal) ? minCVal : null,
            EnableAutoScaling = TryGetNullableBool(poolEl, "enableAutoScaling"),
            ScaleDownMode = poolEl.TryGetProperty("scaleDownMode", out var sdmEl) ? sdmEl.GetString() : null,
            ProvisioningState = poolEl.TryGetProperty("provisioningState", out var ppEl) ? ppEl.GetString() : null,
            PowerState = powerState,
            Mode = poolEl.TryGetProperty("mode", out var poolModeEl) ? poolModeEl.GetString() : null,
            OrchestratorVersion = poolEl.TryGetProperty("orchestratorVersion", out var ovEl) ? ovEl.GetString() : null,
            CurrentOrchestratorVersion = poolEl.TryGetProperty("currentOrchestratorVersion", out var covEl) ? covEl.GetString() : null,
            EnableNodePublicIP = TryGetNullableBool(poolEl, "enableNodePublicIP"),
            ScaleSetPriority = poolEl.TryGetProperty("scaleSetPriority", out var sspEl) ? sspEl.GetString() : null,
            ScaleSetEvictionPolicy = poolEl.TryGetProperty("scaleSetEvictionPolicy", out var ssepEl) ? ssepEl.GetString() : null,
            NodeLabels = ReadStringDictionaryFromProperty(poolEl, "nodeLabels"),
            NodeTaints = nodeTaints,
            OsType = poolEl.TryGetProperty("osType", out var osTypeEl) ? osTypeEl.GetString() : null,
            OsSKU = poolEl.TryGetProperty("osSKU", out var osSkuEl) ? osSkuEl.GetString() : null,
            NodeImageVersion = poolEl.TryGetProperty("nodeImageVersion", out var nivEl) ? nivEl.GetString() : null,
            Tags = ReadStringDictionaryFromProperty(poolEl, "tags"),
            SpotMaxPrice = poolEl.TryGetProperty("spotMaxPrice", out var smpEl) && smpEl.TryGetDouble(out var smpVal) ? smpVal : null,
            WorkloadRuntime = poolEl.TryGetProperty("workloadRuntime", out var wrEl) ? wrEl.GetString() : null,
            EnableEncryptionAtHost = TryGetNullableBool(poolEl, "enableEncryptionAtHost"),
            EnableUltraSSD = TryGetNullableBool(poolEl, "enableUltraSSD"),
            EnableFIPS = TryGetNullableBool(poolEl, "enableFIPS"),
            UpgradeSettings = upgradeSettings,
            NetworkProfile = networkProfile,
            PodSubnetId = poolEl.TryGetProperty("podSubnetID", out var psidEl) ? psidEl.GetString() : null,
            VnetSubnetId = poolEl.TryGetProperty("vnetSubnetID", out var vsidEl) ? vsidEl.GetString() : null,
        };
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
                static kvp =>
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
