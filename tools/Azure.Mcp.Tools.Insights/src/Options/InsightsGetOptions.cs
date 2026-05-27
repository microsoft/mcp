// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Insights.Options;

public class InsightsGetOptions : SubscriptionOptions
{
    [JsonPropertyName(InsightsOptionDefinitions.QueryName)]
    public string? Query { get; set; }

    [JsonPropertyName(InsightsOptionDefinitions.ScopeName)]
    public string? Scope { get; set; }
}
