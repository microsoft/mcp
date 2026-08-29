using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Advisor.Commands.Metadata;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services.Models;

namespace Azure.Mcp.Tools.Advisor.Commands;

[JsonSerializable(typeof(MetadataGetCommand.MetadataGetResult))]
[JsonSerializable(typeof(RecommendationMetadataListCommand.RecommendationMetadataListResult))]
[JsonSerializable(typeof(RecommendationListCommand.RecommendationListResult))]
[JsonSerializable(typeof(RecommendationSummaryCommand.RecommendationSummaryResult))]
[JsonSerializable(typeof(RecommendationApplyCommand.RecommendationApplyCommandResult))]
[JsonSerializable(typeof(RecommendationData))]
[JsonSerializable(typeof(RecommendationMetadataData))]
[JsonSerializable(typeof(RecommendationMetadataDataProperties))]
[JsonSerializable(typeof(RecommendationMetadataActionData))]
[JsonSerializable(typeof(RecommendationMetadataResourceMetadata))]
[JsonSerializable(typeof(RecommendationMetadataServiceHealthData))]
[JsonSerializable(typeof(RecommendationMetadataServiceRetirementData))]
[JsonSerializable(typeof(RecommendationMetadataSourceProperties))]
[JsonSerializable(typeof(Models.Recommendation))]
[JsonSerializable(typeof(Models.RecommendationMetadata))]
[JsonSerializable(typeof(Models.RecommendationGroup))]
[JsonSerializable(typeof(Models.RecommendationSummary))]
[JsonSerializable(typeof(string))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AdvisorJsonContext : JsonSerializerContext;
