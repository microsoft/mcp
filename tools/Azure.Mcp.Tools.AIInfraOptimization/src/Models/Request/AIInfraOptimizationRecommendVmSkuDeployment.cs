// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Request;

public sealed class AIInfraOptimizationRecommendVmSkuDeployment
{
    public int? ReplicaCount { get; set; }

    public string? ProcurementOption { get; set; }
}
