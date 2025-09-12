// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Aks.Services.Models;

internal sealed class AksAgentPoolProfile
{
    /// <summary> Windows agent pool names must be 6 characters or less. </summary>
    public string? Name { get; set; }
    /// <summary> Number of agents (VMs) to host docker containers. Allowed values must be in the range of 0 to 1000 (inclusive) for user pools and in the range of 1 to 1000 (inclusive) for system pools. The default value is 1. </summary>
    public int? Count { get; set; }
    /// <summary> VM size availability varies by region. If a node contains insufficient compute resources (memory, cpu, etc) pods might fail to run correctly. For more details on restricted VM sizes, see: https://docs.microsoft.com/azure/aks/quotas-skus-regions. </summary>
    public string? VmSize { get; set; }
    /// <summary> The operating system type. The default is Linux. </summary>
    [JsonPropertyName("osType")]
    public string? OSType { get; set; }
    /// <summary> A cluster must have at least one 'System' Agent Pool at all times. For additional information on agent pool restrictions and best practices, see: https://docs.microsoft.com/azure/aks/use-system-pools. </summary>
    public string? Mode { get; set; }
    /// <summary> Both patch version &lt;major.minor.patch&gt; (e.g. 1.20.13) and &lt;major.minor&gt; (e.g. 1.20) are supported. When &lt;major.minor&gt; is specified, the latest supported GA patch version is chosen automatically. Updating the cluster with the same &lt;major.minor&gt; once it has been created (e.g. 1.14.x -&gt; 1.14) will not trigger an upgrade, even if a newer patch version is available. As a best practice, you should upgrade all node pools in an AKS cluster to the same Kubernetes version. The node pool version must have the same major version as the control plane. The node pool minor version must be within two minor versions of the control plane version. The node pool version cannot be greater than the control plane version. For more information see [upgrading a node pool](https://docs.microsoft.com/azure/aks/use-multiple-node-pools#upgrade-a-node-pool). </summary>
    public string? OrchestratorVersion { get; set; }
    /// <summary> Whether to enable auto-scaler. </summary>
    public bool? EnableAutoScaling { get; set; }
    /// <summary> The maximum number of nodes for auto-scaling. </summary>
    public int? MaxCount { get; set; }
    /// <summary> The minimum number of nodes for auto-scaling. </summary>
    public int? MinCount { get; set; }
    /// <summary> The current deployment or provisioning state. </summary>
    public string? ProvisioningState { get; set; }
}
