// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Aks.Services.Models;

/// <summary>
/// A class representing the AksClusterProperties data model.
/// </summary>
internal sealed class AksClusterProperties
{
    /// <summary> The current provisioning state. </summary>
    public string? ProvisioningState { get; set; }
    /// <summary> The Power State of the cluster. </summary>
    public AksPowerState? PowerState { get; set; }
    /// <summary> Both patch version &lt;major.minor.patch&gt; (e.g. 1.20.13) and &lt;major.minor&gt; (e.g. 1.20) are supported. When &lt;major.minor&gt; is specified, the latest supported GA patch version is chosen automatically. Updating the cluster with the same &lt;major.minor&gt; once it has been created (e.g. 1.14.x -&gt; 1.14) will not trigger an upgrade, even if a newer patch version is available. When you upgrade a supported AKS cluster, Kubernetes minor versions cannot be skipped. All upgrades must be performed sequentially by major version number. For example, upgrades between 1.14.x -&gt; 1.15.x or 1.15.x -&gt; 1.16.x are allowed, however 1.14.x -&gt; 1.16.x is not allowed. See [upgrading an AKS cluster](https://docs.microsoft.com/azure/aks/upgrade-cluster) for more details. </summary>
    public string? KubernetesVersion { get; set; }
    /// <summary> This cannot be updated once the Managed Cluster has been created. </summary>
    public string? DnsPrefix { get; set; }
    /// <summary> The FQDN of the master pool. </summary>
    public string? Fqdn { get; set; }
    /// <summary> The agent pool properties. </summary>
    public IList<AksAgentPoolProfile>? AgentPoolProfiles { get; set; }
    /// <summary> Whether to enable Kubernetes Role-Based Access Control. </summary>
    [JsonPropertyName("enableRBAC")]
    public bool? EnableRbac { get; set; }
    /// <summary> The network configuration profile. </summary>
    public AksNetworkProfile? NetworkProfile { get; set; }
}
