// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Request;

public sealed class AIInfraOptimizationRecommendVmSkuOptimization
{
    public string? RankingPreference { get; set; }

    public AIInfraOptimizationRecommendVmSkuHourlyCost? MaxDeploymentCostPerHour { get; set; }
}
