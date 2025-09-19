// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public class NodePool
{
    /// <summary> Name of the node pool (agent pool). </summary>
    public string? Name { get; set; }

    /// <summary> Number of nodes in the pool. </summary>
    public int? Count { get; set; }

    /// <summary> VM size of the agent nodes. </summary>
    public string? VmSize { get; set; }

    /// <summary> OS Disk size in GB. </summary>
    public int? OsDiskSizeGB { get; set; }

    /// <summary> OS Disk type (Managed/Ephemeral). </summary>
    public string? OsDiskType { get; set; }

    /// <summary> Kubelet disk type (OS/Temp). </summary>
    public string? KubeletDiskType { get; set; }

    /// <summary> Maximum pods per node. </summary>
    public int? MaxPods { get; set; }

    /// <summary> Pool type (e.g., VirtualMachineScaleSets). </summary>
    public string? Type { get; set; }

    /// <summary> Maximum node count when autoscaling is enabled. </summary>
    public int? MaxCount { get; set; }

    /// <summary> Minimum node count when autoscaling is enabled. </summary>
    public int? MinCount { get; set; }

    /// <summary> Whether cluster autoscaler is enabled for this pool. </summary>
    public bool? EnableAutoScaling { get; set; }

    /// <summary> Scale down mode (e.g., Delete, Deallocate). </summary>
    public string? ScaleDownMode { get; set; }

    /// <summary> Provisioning state of the node pool resource. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> Power state of the node pool. </summary>
    public NodePoolPowerState? PowerState { get; set; }

    /// <summary> Target orchestrator (Kubernetes) version. </summary>
    public string? OrchestratorVersion { get; set; }

    /// <summary> Current orchestrator (Kubernetes) version. </summary>
    public string? CurrentOrchestratorVersion { get; set; }

    /// <summary> Whether nodes have a public IP. </summary>
    public bool? EnableNodePublicIP { get; set; }

    /// <summary> VMSS priority (e.g., Regular, Spot). </summary>
    public string? ScaleSetPriority { get; set; }

    /// <summary> VMSS eviction policy (e.g., Delete, Deallocate). </summary>
    public string? ScaleSetEvictionPolicy { get; set; }

    /// <summary> Node labels. </summary>
    public IDictionary<string, string>? NodeLabels { get; set; }

    /// <summary> Node taints. </summary>
    public IList<string>? NodeTaints { get; set; }

    /// <summary> Pool mode (System/User). </summary>
    public string? Mode { get; set; }

    /// <summary> OS type of the node pool. </summary>
    public string? OsType { get; set; }

    /// <summary> OS SKU (e.g., Ubuntu, CBLMariner). </summary>
    [System.Text.Json.Serialization.JsonPropertyName("osSKU")]
    public string? OsSKU { get; set; }

    /// <summary> Node image version. </summary>
    public string? NodeImageVersion { get; set; }
}

public sealed class NodePoolPowerState
{
    /// <summary> Power state code (e.g., Running, Stopped). </summary>
    public string? Code { get; set; }
}
