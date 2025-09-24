// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.ApplicationInsights.Commands.AppTrace;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Models;

namespace Azure.Mcp.Tools.ApplicationInsights.Commands;

[JsonSerializable(typeof(RecommendationListCommand.RecommendationListCommandResult))]
[JsonSerializable(typeof(AppTraceListCommand.AppTraceListCommandResult))]
[JsonSerializable(typeof(IEnumerable<JsonNode>))]
[JsonSerializable(typeof(BulkAppsPostBody))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ApplicationInsightsJsonContext : JsonSerializerContext
{
}
