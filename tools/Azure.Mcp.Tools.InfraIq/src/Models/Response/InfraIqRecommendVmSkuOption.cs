// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.InfraIq.Models.Response;

public sealed class InfraIqRecommendVmSkuOption
{
    public required string VmSize { get; set; }

    public required int RecommendationRank { get; set; }

    public required InfraIqRecommendVmSkuReadiness Readiness { get; set; }

    public required InfraIqRecommendVmSkuTopology Topology { get; set; }

    public required InfraIqRecommendVmSkuCost Cost { get; set; }

    public InfraIqRecommendVmSkuQuota? Quota { get; set; }

    public InfraIqRecommendVmSkuPlacement? Placement { get; set; }

    public bool? AzureMlRecommended { get; set; }

    public InfraIqRecommendVmSkuBenchmark? Benchmark { get; set; }

    public string? DocumentationUrl { get; set; }

    public InfraIqRecommendVmSkuPerformanceEstimate? PerformanceEstimate { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
