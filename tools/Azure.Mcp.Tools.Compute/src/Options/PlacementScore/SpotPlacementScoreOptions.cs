// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Compute.Options.PlacementScore;

public class SpotPlacementScoreOptions : BaseComputePlacementOptions
{
    [JsonPropertyName(ComputePlacementOptionDefinitions.DesiredLocationsName)]
    public string[]? DesiredLocations { get; set; }

    [JsonPropertyName(ComputePlacementOptionDefinitions.DesiredSizesName)]
    public string[]? DesiredSizes { get; set; }

    [JsonPropertyName(ComputePlacementOptionDefinitions.DesiredCountName)]
    public int DesiredCount { get; set; } = 1;

    [JsonPropertyName(ComputePlacementOptionDefinitions.AvailabilityZonesName)]
    public bool AvailabilityZones { get; set; } = true;
}
