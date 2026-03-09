// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.PlacementScore;

public static class ComputePlacementOptionDefinitions
{
    public const string LocationName = "location";
    public const string DesiredLocationsName = "desired-locations";
    public const string DesiredSizesName = "desired-sizes";
    public const string DesiredCountName = "desired-count";
    public const string AvailabilityZonesName = "availability-zones";

    public static readonly Option<string> Location = new($"--{LocationName}")
    {
        Description = "The ARM region for API routing (e.g., 'eastus', 'westus2'). This is the location used for the placement scores API endpoint.",
        Required = true
    };

    public static readonly Option<string[]> DesiredLocations = new($"--{DesiredLocationsName}")
    {
        Description = "ARM region names to evaluate for placement scores (e.g., 'eastus', 'westus2'). Provide 1-3 regions.",
        Required = true,
        Arity = ArgumentArity.OneOrMore
    };

    public static readonly Option<string[]> DesiredSizes = new($"--{DesiredSizesName}")
    {
        Description = "VM SKU names to evaluate for placement scores (e.g., 'Standard_D2_v2', 'Standard_D4s_v3'). Each value represents a VM size.",
        Required = true,
        Arity = ArgumentArity.OneOrMore
    };

    public static readonly Option<int> DesiredCount = new($"--{DesiredCountName}")
    {
        Description = "Number of VMs to check placement feasibility for. Must be between 1 and 1000. Defaults to 1.",
        Required = false,
        DefaultValueFactory = _ => 1
    };

    public static readonly Option<bool> AvailabilityZones = new($"--{AvailabilityZonesName}")
    {
        Description = "Whether to include availability zone-level placement scores. Defaults to true.",
        Required = false,
        DefaultValueFactory = _ => true
    };
}
