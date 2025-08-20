// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public class Cluster
{
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

    /// <summary> Resource tags associated with the cluster. </summary>
    public IDictionary<string, string>? Tags { get; set; }
}
