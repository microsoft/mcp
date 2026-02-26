// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.WellArchitected.Models;

namespace Azure.Mcp.Tools.WellArchitected.Commands;

[JsonSerializable(typeof(Analyze.AnalyzeCommand.AnalyzeCommandResult))]
[JsonSerializable(typeof(Recommendation.RecommendationListCommand.RecommendationListCommandResult))]
[JsonSerializable(typeof(Recommendation.RecommendationGetCommand.RecommendationGetCommandResult))]
[JsonSerializable(typeof(Checklist.ChecklistGetCommand.ChecklistGetCommandResult))]
[JsonSerializable(typeof(ServiceGuide.ServiceGuideGetCommand.ServiceGuideGetCommandResult))]
[JsonSerializable(typeof(WafRecommendation))]
[JsonSerializable(typeof(WafChecklist))]
[JsonSerializable(typeof(WafChecklistItem))]
[JsonSerializable(typeof(WafServiceGuide))]
[JsonSerializable(typeof(WafAnalysisResult))]
[JsonSerializable(typeof(WafContentIndex))]
[JsonSerializable(typeof(WafAnalyzeResponse))]
[JsonSerializable(typeof(AnalysisContext))]
[JsonSerializable(typeof(PropertySignals))]
[JsonSerializable(typeof(WafGuidance))]
[JsonSerializable(typeof(ServiceGuideMatch))]
[JsonSerializable(typeof(PillarRecommendations))]
[JsonSerializable(typeof(RelevantRecommendation))]
[JsonSerializable(typeof(Dictionary<string, WafRecommendation>))]
[JsonSerializable(typeof(Dictionary<string, WafChecklist>))]
[JsonSerializable(typeof(Dictionary<string, WafServiceGuide>))]
[JsonSerializable(typeof(Dictionary<string, PillarRecommendations>))]
[JsonSerializable(typeof(Dictionary<string, List<string>>))]
[JsonSerializable(typeof(List<WafRecommendation>))]
[JsonSerializable(typeof(List<WafAnalysisResult>))]
[JsonSerializable(typeof(List<WafChecklistItem>))]
[JsonSerializable(typeof(List<ServiceGuideMatch>))]
[JsonSerializable(typeof(List<RelevantRecommendation>))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(JsonElement))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault)]
internal sealed partial class WellArchitectedJsonContext : JsonSerializerContext
{
}
