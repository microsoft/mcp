// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;

namespace Azure.Mcp.Tools.ApplicationInsights.Commands;

[JsonSerializable(typeof(RecommendationListCommand.RecommendationListCommandResult))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ApplicationInsightsJsonContext : JsonSerializerContext
{
}
