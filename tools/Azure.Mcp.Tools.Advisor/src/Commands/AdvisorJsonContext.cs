using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services.Models;

namespace Azure.Mcp.Tools.Advisor.Commands;

[JsonSerializable(typeof(RecommendationListCommand.RecommendationListResult))]
[JsonSerializable(typeof(RecommendationTypeListCommand.RecommendationTypeListResult))]
[JsonSerializable(typeof(RecommendationSummaryCommand.RecommendationSummaryResult))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(RecommendationData))]
[JsonSerializable(typeof(Models.Recommendation))]
[JsonSerializable(typeof(Models.RecommendationType))]
[JsonSerializable(typeof(RecommendationMetadataApiResponse))]
[JsonSerializable(typeof(Models.RecommendationGroup))]
[JsonSerializable(typeof(Models.RecommendationSummary))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AdvisorJsonContext : JsonSerializerContext;
