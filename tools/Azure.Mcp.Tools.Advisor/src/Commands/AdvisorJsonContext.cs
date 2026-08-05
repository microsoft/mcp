using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Advisor.Commands.Metadata;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services.Models;

namespace Azure.Mcp.Tools.Advisor.Commands;

[JsonSerializable(typeof(RecommendationListCommand.RecommendationListResult))]
[JsonSerializable(typeof(RecommendationTypeListCommand.RecommendationTypeListResult))]
[JsonSerializable(typeof(RecommendationSummaryCommand.RecommendationSummaryResult))]
[JsonSerializable(typeof(MetadataGetCommand.MetadataGetResult))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(RecommendationData))]
[JsonSerializable(typeof(RecommendationMetadataData))]
[JsonSerializable(typeof(RecommendationMetadataDataProperties))]
[JsonSerializable(typeof(RecommendationMetadataResourceMetadata))]
[JsonSerializable(typeof(RecommendationMetadataActionData))]
[JsonSerializable(typeof(RecommendationMetadataSourceProperties))]
[JsonSerializable(typeof(RecommendationMetadataServiceRetirementData))]
[JsonSerializable(typeof(RecommendationMetadataServiceHealthData))]
[JsonSerializable(typeof(Models.Recommendation))]
[JsonSerializable(typeof(Models.RecommendationType))]
[JsonSerializable(typeof(Models.RecommendationMetadata))]
[JsonSerializable(typeof(RecommendationMetadataApiResponse))]
[JsonSerializable(typeof(Models.RecommendationGroup))]
[JsonSerializable(typeof(Models.RecommendationSummary))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AdvisorJsonContext : JsonSerializerContext;
