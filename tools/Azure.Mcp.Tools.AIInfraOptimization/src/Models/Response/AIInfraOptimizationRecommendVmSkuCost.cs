// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AIInfraOptimization.Models.Response;

public sealed class AIInfraOptimizationRecommendVmSkuCost
{
    public required string CurrencyCode { get; set; }

    public required double VmHourly { get; set; }

    public required double DeploymentHourly { get; set; }

    public required string Source { get; set; }

    public DateTimeOffset? PriceAsOf { get; set; }

    public AIInfraOptimizationRecommendVmSkuStageError? Error { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
