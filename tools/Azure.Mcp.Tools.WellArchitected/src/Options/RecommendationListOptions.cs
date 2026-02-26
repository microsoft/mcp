// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.WellArchitected.Options;

public class RecommendationListOptions : GlobalOptions
{
    [JsonPropertyName(WellArchitectedOptionDefinitions.PillarName)]
    public string? Pillar { get; set; }

    [JsonPropertyName(WellArchitectedOptionDefinitions.ServiceName)]
    public string? Service { get; set; }
}
