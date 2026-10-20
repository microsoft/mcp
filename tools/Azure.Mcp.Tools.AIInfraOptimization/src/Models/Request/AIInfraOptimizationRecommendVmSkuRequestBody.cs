// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Request;

public sealed class AIInfraOptimizationRecommendVmSkuRequestBody
{
    public AIInfraOptimizationRecommendVmSkuModel? Model { get; set; }

    public AIInfraOptimizationRecommendVmSkuWorkload? Workload { get; set; }

    public AIInfraOptimizationRecommendVmSkuDeployment? Deployment { get; set; }

    public AIInfraOptimizationRecommendVmSkuOptimization? Optimization { get; set; }

    public List<string>? TargetVmSizes { get; set; }

    public AIInfraOptimizationRecommendVmSkuSubscriptionOptions? SubscriptionOptions { get; set; }
}
