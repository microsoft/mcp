using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services.Models;

namespace Azure.Mcp.Tools.Advisor.Commands;

[JsonSerializable(typeof(RecommendationListCommand.RecommendationListResult))]
[JsonSerializable(typeof(RecommendationData))]
[JsonSerializable(typeof(Models.Recommendation))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class AdvisorJsonContext : JsonSerializerContext;
