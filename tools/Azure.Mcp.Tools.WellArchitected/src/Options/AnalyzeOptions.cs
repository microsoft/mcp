// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.WellArchitected.Options;

public class AnalyzeOptions : GlobalOptions
{
    [JsonPropertyName(WellArchitectedOptionDefinitions.InfrastructureConfigName)]
    public string? InfrastructureConfig { get; set; }

    [JsonPropertyName(WellArchitectedOptionDefinitions.IntentName)]
    public string? Intent { get; set; }
}
