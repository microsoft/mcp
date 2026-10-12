// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Response;

public sealed class AIInfraOptimizationRecommendVmSkuTopology
{
    public required int EstimatedNodeCountPerReplica { get; set; }

    public required long EstimatedTotalNodeCount { get; set; }

    public required AIInfraOptimizationRecommendVmSkuAccelerator Accelerator { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
