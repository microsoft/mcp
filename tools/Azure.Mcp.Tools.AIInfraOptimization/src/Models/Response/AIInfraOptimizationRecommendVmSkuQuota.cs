// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Response;

public sealed class AIInfraOptimizationRecommendVmSkuQuota
{
    public required string Status { get; set; }

    public string? Reason { get; set; }

    public AIInfraOptimizationRecommendVmSkuStageError? Error { get; set; }

    public int? MaxAvailableReplicaCount { get; set; }

    public AIInfraOptimizationRecommendVmSkuQuotaIncrease? Increase { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
