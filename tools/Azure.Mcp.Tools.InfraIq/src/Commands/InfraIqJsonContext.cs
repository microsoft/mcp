// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.InfraIq.Models.Common;
using Azure.Mcp.Tools.InfraIq.Models.Error;
using Azure.Mcp.Tools.InfraIq.Models.Request;
using Azure.Mcp.Tools.InfraIq.Models.Response;
using Azure.Mcp.Tools.InfraIq.Models.VmSku;

namespace Azure.Mcp.Tools.InfraIq.Commands;

[JsonSerializable(typeof(VmSkuRecommendResult))]
[JsonSerializable(typeof(VmSkuRecommendErrorResult))]
[JsonSerializable(typeof(InfraIqArmResponseContext))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuRequestBody))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuModel))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuWorkload))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuTokenBudget))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuDeployment))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuOptimization))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuHourlyCost))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuSubscriptionOptions))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuResponse))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuOption))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuReadiness))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuTopology))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuAccelerator))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuCost))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuStageError))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuQuota))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuQuotaIncrease))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuQuotaLimit))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuPlacement))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuBenchmark))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuPerformanceEstimate))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuSizing))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuSizingModel))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuSizingWorkload))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuMemory))]
[JsonSerializable(typeof(InfraIqRecommendVmSkuSizingBasis))]
[JsonSerializable(typeof(InfraIqArmODataErrorEnvelope))]
[JsonSerializable(typeof(InfraIqArmODataError))]
[JsonSerializable(typeof(Dictionary<string, JsonElement>))]
[JsonSerializable(typeof(List<string>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class InfraIqJsonContext : JsonSerializerContext;
