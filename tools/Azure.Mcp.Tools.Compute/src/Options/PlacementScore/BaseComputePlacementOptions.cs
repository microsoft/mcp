// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.PlacementScore;

public class BaseComputePlacementOptions : SubscriptionOptions
{
    [JsonPropertyName(ComputePlacementOptionDefinitions.LocationName)]
    public string? Location { get; set; }
}
