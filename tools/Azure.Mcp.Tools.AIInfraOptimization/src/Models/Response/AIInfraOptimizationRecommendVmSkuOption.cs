// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Response;

public sealed class AIInfraOptimizationRecommendVmSkuOption
{
    public required string VmSize { get; set; }

    public required int RecommendationRank { get; set; }

    public required AIInfraOptimizationRecommendVmSkuReadiness Readiness { get; set; }

    public required AIInfraOptimizationRecommendVmSkuTopology Topology { get; set; }

    public required AIInfraOptimizationRecommendVmSkuCost Cost { get; set; }

    public AIInfraOptimizationRecommendVmSkuQuota? Quota { get; set; }

    public AIInfraOptimizationRecommendVmSkuPlacement? Placement { get; set; }

    public bool? AzureMlRecommended { get; set; }

    public AIInfraOptimizationRecommendVmSkuBenchmark? Benchmark { get; set; }

    public string? DocumentationUrl { get; set; }

    public AIInfraOptimizationRecommendVmSkuPerformanceEstimate? PerformanceEstimate { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
