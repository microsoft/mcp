// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Request;

public sealed class AIInfraOptimizationRecommendVmSkuTokenBudget
{
    public int? PromptTokens { get; set; }

    public int? MaxOutputTokens { get; set; }
}
