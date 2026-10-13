// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Optimization.Commands.Recommendation;
using Azure.Mcp.Tools.Optimization.Models;

namespace Azure.Mcp.Tools.Optimization.Commands;

[JsonSerializable(typeof(RecommendationListCommand.RecommendationListResult))]
[JsonSerializable(typeof(RecommendationAlternativesCommand.RecommendationAlternativesResult))]
[JsonSerializable(typeof(RecommendationExplanationResult))]
[JsonSerializable(typeof(AlternativeRecommendation))]
[JsonSerializable(typeof(CostSavingsRecommendation))]
[JsonSerializable(typeof(SubscriptionOption))]
[JsonSerializable(typeof(RecommendationUtilization))]
[JsonSerializable(typeof(SkuConfiguration))]
[JsonSerializable(typeof(UtilizationThresholds))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class OptimizationJsonContext : JsonSerializerContext;
