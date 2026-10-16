// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Request;

public sealed class AIInfraOptimizationRecommendVmSkuSubscriptionOptions
{
    public bool IncludeQuota { get; set; }

    public bool IncludePlacement { get; set; }

    public bool IncludePricing { get; set; }
}
