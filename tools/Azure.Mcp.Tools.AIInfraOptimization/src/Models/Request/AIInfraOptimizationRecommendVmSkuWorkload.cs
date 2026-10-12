// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Request;

public sealed class AIInfraOptimizationRecommendVmSkuWorkload
{
    public int? MaxConcurrentRequestsPerReplica { get; set; }

    public AIInfraOptimizationRecommendVmSkuTokenBudget? TokenBudget { get; set; }
}
