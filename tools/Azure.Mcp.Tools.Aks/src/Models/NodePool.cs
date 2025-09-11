// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public class NodePool
{
    /// <summary> Name of the node pool (agent pool). </summary>
    public string? Name { get; set; }

    /// <summary> Number of nodes in the pool. </summary>
    public int? NodeCount { get; set; }

    /// <summary> VM size of the agent nodes. </summary>
    public string? NodeVmSize { get; set; }

    /// <summary> OS type of the node pool. </summary>
    public string? OsType { get; set; }

    /// <summary> Pool mode (System/User). </summary>
    public string? Mode { get; set; }

    /// <summary> Kubernetes/orchestrator version for the pool. </summary>
    public string? OrchestratorVersion { get; set; }

    /// <summary> Whether cluster autoscaler is enabled for this pool. </summary>
    public bool? EnableAutoScaling { get; set; }

    /// <summary> Minimum node count when autoscaling is enabled. </summary>
    public int? MinCount { get; set; }

    /// <summary> Maximum node count when autoscaling is enabled. </summary>
    public int? MaxCount { get; set; }

    /// <summary> Provisioning state of the node pool resource. </summary>
    public string? ProvisioningState { get; set; }
}

