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
    /// <summary> OS Disk Size in GB to be used to specify the disk size for every machine in the master/agent pool. If you specify 0, it will apply the default osDisk size according to the vmSize specified. </summary>
    [JsonPropertyName("osDiskSizeGB")]
    public int? OSDiskSizeInGB { get; set; }
    /// <summary> The default is 'Ephemeral' if the VM supports it and has a cache disk larger than the requested OSDiskSizeGB. Otherwise, defaults to 'Managed'. May not be changed after creation. For more information see [Ephemeral OS](https://docs.microsoft.com/azure/aks/cluster-configuration#ephemeral-os). </summary>
    [JsonPropertyName("osDiskType")]
    public string? OSDiskType { get; set; }
    /// <summary> Determines the placement of emptyDir volumes, container runtime data root, and Kubelet ephemeral storage. </summary>
    public string? KubeletDiskType { get; set; }
    /// <summary> The maximum number of pods that can run on a node. </summary>
    public int? MaxPods { get; set; }
    /// <summary> The type of Agent Pool. </summary>
    [JsonPropertyName("type")]
    public string? AgentPoolType { get; set; }
    /// <summary> The maximum number of nodes for auto-scaling. </summary>
    public int? MaxCount { get; set; }
    /// <summary> The minimum number of nodes for auto-scaling. </summary>
    public int? MinCount { get; set; }
    /// <summary> Whether to enable auto-scaler. </summary>
    public bool? EnableAutoScaling { get; set; }
    /// <summary> This also effects the cluster autoscaler behavior. If not specified, it defaults to Delete. </summary>
    public string? ScaleDownMode { get; set; }
    /// <summary> The current deployment or provisioning state. </summary>
    public string? ProvisioningState { get; set; }
    /// <summary> Tells whether the cluster is Running or Stopped. </summary>
    public AksPowerState? PowerState { get; set; }
    /// <summary> Both patch version &lt;major.minor.patch&gt; (e.g. 1.20.13) and &lt;major.minor&gt; (e.g. 1.20) are supported. When &lt;major.minor&gt; is specified, the latest supported GA patch version is chosen automatically. Updating the cluster with the same &lt;major.minor&gt; once it has been created (e.g. 1.14.x -&gt; 1.14) will not trigger an upgrade, even if a newer patch version is available. As a best practice, you should upgrade all node pools in an AKS cluster to the same Kubernetes version. The node pool version must have the same major version as the control plane. The node pool minor version must be within two minor versions of the control plane version. The node pool version cannot be greater than the control plane version. For more information see [upgrading a node pool](https://docs.microsoft.com/azure/aks/use-multiple-node-pools#upgrade-a-node-pool). </summary>
    public string? OrchestratorVersion { get; set; }
    /// <summary> If orchestratorVersion is a fully specified version &lt;major.minor.patch&gt;, this field will be exactly equal to it. If orchestratorVersion is &lt;major.minor&gt;, this field will contain the full &lt;major.minor.patch&gt; version being used. </summary>
    public string? CurrentOrchestratorVersion { get; set; }
    /// <summary> Some scenarios may require nodes in a node pool to receive their own dedicated public IP addresses. A common scenario is for gaming workloads, where a console needs to make a direct connection to a cloud virtual machine to minimize hops. For more information see [assigning a public IP per node](https://docs.microsoft.com/azure/aks/use-multiple-node-pools#assign-a-public-ip-per-node-for-your-node-pools). The default is false. </summary>
    public bool? EnableNodePublicIP { get; set; }
    /// <summary> The Virtual Machine Scale Set priority. If not specified, the default is 'Regular'. </summary>
    public string? ScaleSetPriority { get; set; }
    /// <summary> This cannot be specified unless the scaleSetPriority is 'Spot'. If not specified, the default is 'Delete'. </summary>
    public string? ScaleSetEvictionPolicy { get; set; }
    /// <summary> The node labels to be persisted across all nodes in agent pool. </summary>
    public IDictionary<string, string>? NodeLabels { get; }
    /// <summary> The taints added to new nodes during node pool create and scale. For example, key=value:NoSchedule. </summary>
    public IList<string>? NodeTaints { get; }
    /// <summary> A cluster must have at least one 'System' Agent Pool at all times. For additional information on agent pool restrictions and best practices, see: https://docs.microsoft.com/azure/aks/use-system-pools. </summary>
    public string? Mode { get; set; }
    /// <summary> The operating system type. The default is Linux. </summary>
    [JsonPropertyName("osType")]
    public string? OSType { get; set; }
    /// <summary> Specifies the OS SKU used by the agent pool. The default is Ubuntu if OSType is Linux. The default is Windows2019 when Kubernetes &lt;= 1.24 or Windows2022 when Kubernetes &gt;= 1.25 if OSType is Windows. </summary>
    [JsonPropertyName("osSKU")]
    public string? OSSku { get; set; }
    /// <summary> The version of node image. </summary>
    public string? NodeImageVersion { get; set; }
}
