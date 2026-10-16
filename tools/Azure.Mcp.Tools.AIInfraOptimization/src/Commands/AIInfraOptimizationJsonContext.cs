// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Common;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Error;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Request;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Response;
using Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;

namespace Azure.Mcp.Tools.AIInfraOptimization.Commands;

[JsonSerializable(typeof(VmSkuRecommendResult))]
[JsonSerializable(typeof(VmSkuRecommendErrorResult))]
[JsonSerializable(typeof(AIInfraOptimizationArmResponseContext))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuRequestBody))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuModel))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuWorkload))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuTokenBudget))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuDeployment))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuOptimization))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuHourlyCost))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuSubscriptionOptions))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuResponse))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuOption))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuReadiness))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuTopology))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuAccelerator))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuCost))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuStageError))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuQuota))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuQuotaIncrease))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuQuotaLimit))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuPlacement))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuBenchmark))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuPerformanceEstimate))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuSizing))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuSizingModel))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuSizingWorkload))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuMemory))]
[JsonSerializable(typeof(AIInfraOptimizationRecommendVmSkuSizingBasis))]
[JsonSerializable(typeof(AIInfraOptimizationArmODataErrorEnvelope))]
[JsonSerializable(typeof(AIInfraOptimizationArmODataError))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(List<string>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AIInfraOptimizationJsonContext : JsonSerializerContext;
