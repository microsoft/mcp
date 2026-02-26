// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.WellArchitected.Options;

public class RecommendationGetOptions : GlobalOptions
{
    [JsonPropertyName(WellArchitectedOptionDefinitions.RecommendationIdName)]
    public string? RecommendationId { get; set; }
}
