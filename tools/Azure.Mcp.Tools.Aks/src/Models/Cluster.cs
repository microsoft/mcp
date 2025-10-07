// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public class Cluster
{
    /// <summary> Azure resource ID of the AKS cluster. </summary>
    public string? Id { get; set; }

    /// <summary> Name of the AKS cluster resource. </summary>
    public string? Name { get; set; }

    /// <summary> ID of the Azure subscription containing the AKS cluster resource. </summary>
    public string? SubscriptionId { get; set; }

    /// <summary> Name of the resource group containing the AKS cluster resource. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> Azure geo-location where the AKS cluster resource lives. </summary>
    public string? Location { get; set; }

    /// <summary> Kubernetes version of the AKS cluster. </summary>
    public string? KubernetesVersion { get; set; }

    /// <summary> Provisioning status of the AKS cluster resource. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> Current power state of the AKS cluster. </summary>
    public string? PowerState { get; set; }

    /// <summary> DNS prefix specified when creating the managed cluster. </summary>
    public string? DnsPrefix { get; set; }

    /// <summary> FQDN for the master pool. </summary>
    public string? Fqdn { get; set; }

    /// <summary> Number of nodes in the default agent pool. </summary>
    public int? NodeCount { get; set; }

    /// <summary> VM size of the agent nodes. </summary>
    public string? NodeVmSize { get; set; }

    /// <summary> Type of managed identity used by this managed cluster. </summary>
    public string? IdentityType { get; set; }

    /// <summary> Managed identity details of this managed cluster. </summary>
    public ResourceIdentity? Identity { get; set; }

    /// <summary> Whether RBAC is enabled. </summary>
    public bool? EnableRbac { get; set; }

    /// <summary> Network plugin used for building the Kubernetes network. </summary>
    public string? NetworkPlugin { get; set; }

    /// <summary> Network policy used for building the Kubernetes network. </summary>
    public string? NetworkPolicy { get; set; }

    /// <summary> Service CIDR for the Kubernetes service. </summary>
    public string? ServiceCidr { get; set; }

    /// <summary> DNS service IP address for the Kubernetes service. </summary>
    public string? DnsServiceIP { get; set; }

    /// <summary> SKU tier of this managed cluster. </summary>
    public string? SkuTier { get; set; }

    /// <summary> SKU name of this managed cluster. </summary>
    public string? SkuName { get; set; }

    /// <summary> Node resource group created by AKS. </summary>
    public string? NodeResourceGroup { get; set; }

    /// <summary> Maximum agent pools supported by the cluster. </summary>
    public int? MaxAgentPools { get; set; }

    /// <summary> Support plan for the cluster. </summary>
    public string? SupportPlan { get; set; }

    /// <summary> Rich network profile. </summary>
    public ClusterNetworkProfile? NetworkProfile { get; set; }

    /// <summary> Windows profile settings. </summary>
    public WindowsProfile? WindowsProfile { get; set; }

    /// <summary> Service principal profile. </summary>
    public ServicePrincipalProfile? ServicePrincipalProfile { get; set; }

    /// <summary> OIDC issuer profile. </summary>
    public OidcIssuerProfile? OidcIssuerProfile { get; set; }

    /// <summary> Cluster auto-upgrade profile. </summary>
    public AutoUpgradeProfile? AutoUpgradeProfile { get; set; }

    /// <summary> Cluster security profile. </summary>
    public ClusterSecurityProfile? SecurityProfile { get; set; }

    /// <summary> Cluster storage profile. </summary>
    public ClusterStorageProfile? StorageProfile { get; set; }

    /// <summary> Cluster metrics profile. </summary>
    public ClusterMetricsProfile? MetricsProfile { get; set; }

    /// <summary> Cluster autoscaler profile key-values. </summary>
    public IDictionary<string, string>? AutoScalerProfile { get; set; }

    /// <summary> Node provisioning profile. </summary>
    public ClusterNodeProvisioningProfile? NodeProvisioningProfile { get; set; }

    /// <summary> Bootstrap profile. </summary>
    public ClusterBootstrapProfile? BootstrapProfile { get; set; }

    /// <summary> Workload auto-scaler profile. </summary>
    public WorkloadAutoScalerProfile? WorkloadAutoScalerProfile { get; set; }

    /// <summary> AI toolchain operator profile. </summary>
    public AiToolchainOperatorProfile? AiToolchainOperatorProfile { get; set; }

    /// <summary> Add-on profiles keyed by add-on name (general string map per add-on). </summary>
    public IDictionary<string, IDictionary<string, string>>? AddonProfiles { get; set; }

    /// <summary> Identity profile references keyed by component name. </summary>
    public IDictionary<string, ManagedIdentityReference>? IdentityProfile { get; set; }

    /// <summary> Whether local accounts are disabled. </summary>
    public bool? DisableLocalAccounts { get; set; }

    /// <summary> Unique resource UID returned by the service. </summary>
    public string? ResourceUid { get; set; }

    /// <summary> Agent pool profiles for the cluster. </summary>
    public IList<NodePool>? AgentPoolProfiles { get; set; }

    /// <summary> Resource tags associated with the cluster. </summary>
    public IDictionary<string, string>? Tags { get; set; }
}
