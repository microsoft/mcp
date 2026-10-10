// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuOptimization
{
    public string? RankingPreference { get; set; }

    public InfraIqRecommendVmSkuHourlyCost? MaxDeploymentCostPerHour { get; set; }
}
