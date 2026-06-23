// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Models;

namespace Azure.Mcp.Tools.Aks.Services;

/// <summary>
/// Intermediate type that mirrors the Azure Resource Graph JSON structure for a managed cluster
/// resource. Used only for deserialization; the public <see cref="Cluster"/> model is then
/// populated from this type via <c>AksService.MapToCluster</c>.
/// </summary>
internal sealed class AksClusterResourceGraphResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? SubscriptionId { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    public string? Location { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
    public AksClusterSkuJson? Sku { get; set; }
    public ResourceIdentity? Identity { get; set; }
    public AksClusterPropertiesJson? Properties { get; set; }
}

internal sealed class AksClusterSkuJson
{
    public string? Name { get; set; }
    public string? Tier { get; set; }
}

internal sealed class AksClusterPropertiesJson
{
    public string? KubernetesVersion { get; set; }
    public string? ProvisioningState { get; set; }
    public string? DnsPrefix { get; set; }
    public string? Fqdn { get; set; }
    public string? NodeResourceGroup { get; set; }
    public string? SupportPlan { get; set; }

    [JsonPropertyName("resourceUID")]
    public string? ResourceUid { get; set; }

    [JsonPropertyName("enableRBAC")]
    public bool? EnableRbac { get; set; }

    public bool? DisableLocalAccounts { get; set; }
    public int? MaxAgentPools { get; set; }
    public AksPowerStateJson? PowerState { get; set; }
    public AksClusterNetworkProfileJson? NetworkProfile { get; set; }
    public OidcIssuerProfile? OidcIssuerProfile { get; set; }
    public AutoUpgradeProfile? AutoUpgradeProfile { get; set; }
    public ClusterSecurityProfile? SecurityProfile { get; set; }
    public ClusterStorageProfile? StorageProfile { get; set; }
    public WorkloadAutoScalerProfile? WorkloadAutoScalerProfile { get; set; }
    public Dictionary<string, AksAddonProfileJson>? AddonProfiles { get; set; }
    public Dictionary<string, ManagedIdentityReference>? IdentityProfile { get; set; }
    public List<NodePool>? AgentPoolProfiles { get; set; }
}

internal sealed class AksPowerStateJson
{
    public string? Code { get; set; }
}

/// <summary>
/// Intermediate network profile type. The public <see cref="ClusterNetworkProfile"/> model
/// has a flat <c>ManagedOutboundIPCount</c>, while the Resource Graph JSON nests this value
/// under <c>loadBalancerProfile.managedOutboundIPs.count</c>, requiring a separate type.
/// </summary>
internal sealed class AksClusterNetworkProfileJson
{
    public string? NetworkPlugin { get; set; }
    public string? NetworkPluginMode { get; set; }
    public string? NetworkPolicy { get; set; }
    public string? NetworkDataplane { get; set; }
    public string? LoadBalancerSku { get; set; }
    public AksNetworkLoadBalancerProfileJson? LoadBalancerProfile { get; set; }
    public string? PodCidr { get; set; }
    public string? ServiceCidr { get; set; }
    public string? DnsServiceIP { get; set; }
    public string? OutboundType { get; set; }
    public List<string>? PodCidrs { get; set; }
    public List<string>? ServiceCidrs { get; set; }
    public List<string>? IpFamilies { get; set; }
}

internal sealed class AksNetworkLoadBalancerProfileJson
{
    public AksManagedOutboundIPsJson? ManagedOutboundIPs { get; set; }
    public List<EffectiveOutboundIPReference>? EffectiveOutboundIPs { get; set; }
    public string? BackendPoolType { get; set; }
}

internal sealed class AksManagedOutboundIPsJson
{
    public int? Count { get; set; }
}

/// <summary>
/// Intermediate add-on profile type used to deserialize each entry in
/// <c>properties.addonProfiles</c> before flattening to the <c>config.*</c> /
/// <c>identity.*</c> key convention used by <see cref="Cluster.AddonProfiles"/>.
/// </summary>
internal sealed class AksAddonProfileJson
{
    public bool? Enabled { get; set; }
    public Dictionary<string, string>? Config { get; set; }
    public AksAddonIdentityJson? Identity { get; set; }
}

internal sealed class AksAddonIdentityJson
{
    public string? ClientId { get; set; }
    public string? ObjectId { get; set; }
}
