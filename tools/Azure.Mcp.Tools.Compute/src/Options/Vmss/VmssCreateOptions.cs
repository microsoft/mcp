// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public class VmssCreateOptions : BaseComputeOptions
{
    public string? VmssName { get; set; }

    public string? Location { get; set; }

    public string? VmSize { get; set; }

    public string? Image { get; set; }

    public string? AdminUsername { get; set; }

    public string? AdminPassword { get; set; }

    public string? SshPublicKey { get; set; }

    public string? Workload { get; set; }

    public string? OsType { get; set; }

    public string? VirtualNetwork { get; set; }

    public string? Subnet { get; set; }

    public int? InstanceCount { get; set; }

    public string? UpgradePolicy { get; set; }

    public string? Zone { get; set; }

    public int? OsDiskSizeGb { get; set; }

    public string? OsDiskType { get; set; }
}
