// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ComputeRecommender.Options.PlacementScore;

public class SpotPlacementScoreOptions : BaseComputeRecommenderOptions
{
    [JsonPropertyName(ComputeRecommenderOptionDefinitions.DesiredLocationsName)]
    public string[]? DesiredLocations { get; set; }

    [JsonPropertyName(ComputeRecommenderOptionDefinitions.DesiredSizesName)]
    public string[]? DesiredSizes { get; set; }

    [JsonPropertyName(ComputeRecommenderOptionDefinitions.DesiredCountName)]
    public int DesiredCount { get; set; } = 1;

    [JsonPropertyName(ComputeRecommenderOptionDefinitions.AvailabilityZonesName)]
    public bool AvailabilityZones { get; set; } = true;
}
