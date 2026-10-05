// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuRequestBody
{
    public InfraIqRecommendVmSkuModel? Model { get; set; }

    public InfraIqRecommendVmSkuWorkload? Workload { get; set; }

    public InfraIqRecommendVmSkuDeployment? Deployment { get; set; }

    public InfraIqRecommendVmSkuOptimization? Optimization { get; set; }

    public List<string>? TargetVmSizes { get; set; }

    public InfraIqRecommendVmSkuSubscriptionOptions? SubscriptionOptions { get; set; }
}
