// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuDeployment
{
    public int? ReplicaCount { get; set; }

    public string? ProcurementOption { get; set; }
}
