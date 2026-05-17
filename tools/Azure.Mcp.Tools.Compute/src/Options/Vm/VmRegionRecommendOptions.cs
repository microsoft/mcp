// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmRegionRecommendOptions : BaseComputeOptions
{
    public string? WorkloadHint { get; set; }
    public string? GeographyPreference { get; set; }
    public bool RequireAvailabilityZones { get; set; }
    public int? Top { get; set; }
}
