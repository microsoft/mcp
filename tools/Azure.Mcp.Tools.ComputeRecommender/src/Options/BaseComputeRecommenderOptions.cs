// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ComputeRecommender.Options;

public class BaseComputeRecommenderOptions : SubscriptionOptions
{
    [JsonPropertyName(ComputeRecommenderOptionDefinitions.LocationName)]
    public string? Location { get; set; }
}
