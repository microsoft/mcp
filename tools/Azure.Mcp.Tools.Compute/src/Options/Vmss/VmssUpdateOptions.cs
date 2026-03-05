// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public class VmssUpdateOptions : BaseComputeOptions
{
    public string? VmssName { get; set; }

    public string? VmSize { get; set; }

    public int? Capacity { get; set; }

    public string? UpgradePolicy { get; set; }

    public bool? Overprovision { get; set; }

    public bool? EnableAutoOsUpgrade { get; set; }

    public string? ScaleInPolicy { get; set; }

    public string? Tags { get; set; }
}
