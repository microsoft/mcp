// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmCreateOptions : BaseComputeOptions
{
    public string? VmName { get; set; }

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

    public string? PublicIpAddress { get; set; }

    public string? NetworkSecurityGroup { get; set; }

    public bool? NoPublicIp { get; set; }

    public string? Zone { get; set; }

    public int? OsDiskSizeGb { get; set; }

    public string? OsDiskType { get; set; }
}
