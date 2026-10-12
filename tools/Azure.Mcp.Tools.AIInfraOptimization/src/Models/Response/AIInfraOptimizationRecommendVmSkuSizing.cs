// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Response;

public sealed class AIInfraOptimizationRecommendVmSkuSizing
{
    public required AIInfraOptimizationRecommendVmSkuSizingModel Model { get; set; }

    public required AIInfraOptimizationRecommendVmSkuSizingWorkload Workload { get; set; }

    public required AIInfraOptimizationRecommendVmSkuMemory Memory { get; set; }

    public required AIInfraOptimizationRecommendVmSkuSizingBasis Basis { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
