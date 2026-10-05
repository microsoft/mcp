// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Request;

public sealed class InfraIqRecommendVmSkuTokenBudget
{
    public int? PromptTokens { get; set; }

    public int? MaxOutputTokens { get; set; }
}
