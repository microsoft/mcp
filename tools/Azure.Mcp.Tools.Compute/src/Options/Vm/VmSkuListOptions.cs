// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmSkuListOptions : BaseComputeOptions
{
    public string? Location { get; set; }
    public int? MinVCpus { get; set; }
    public double? MinMemoryGb { get; set; }
    public string? FamilyPrefix { get; set; }
    public int? Top { get; set; }
    public bool IncludePricing { get; set; }
}
