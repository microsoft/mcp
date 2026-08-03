// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public sealed class VmCreateOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmName, Aliases = ["name"])]
    public required string VmName { get; set; }

    [Option(Description = ComputeOptionDescriptions.Location, Aliases = ["l"])]
    public required string Location { get; set; }

    [Option(Description = ComputeOptionDescriptions.VmSize, Aliases = ["size"])]
    public string? VmSize { get; set; }

    [Option(Description = ComputeOptionDescriptions.Image)]
    public required string Image { get; set; }

    [Option(Description = ComputeOptionDescriptions.AdminUsername)]
    public required string AdminUsername { get; set; }

    [Option(Description = ComputeOptionDescriptions.AdminPassword)]
    public string? AdminPassword { get; set; }

    [Option(Description = ComputeOptionDescriptions.SshPublicKey)]
    public string? SshPublicKey { get; set; }

    [Option(Description = ComputeOptionDescriptions.OsType)]
    public string? OsType { get; set; }

    [Option(Description = ComputeOptionDescriptions.VirtualNetwork, Aliases = ["vnet"])]
    public string? VirtualNetwork { get; set; }

    [Option(Description = ComputeOptionDescriptions.Subnet)]
    public string? Subnet { get; set; }

    [Option(Description = "Name of the public IP address to use or create")]
    public string? PublicIpAddress { get; set; }

    [Option(Description = "Name of the network security group to use or create.", Aliases = ["nsg"])]
    public string? NetworkSecurityGroup { get; set; }

    [Option(Description = "Do not create or assign a public IP address")]
    public bool? NoPublicIp { get; set; }

    [Option(Description = ComputeOptionDescriptions.Zone)]
    public string? Zone { get; set; }

    [Option(Description = ComputeOptionDescriptions.OsDiskSizeGb)]
    public int? OsDiskSizeGb { get; set; }

    [Option(Description = ComputeOptionDescriptions.OsDiskType)]
    public string? OsDiskType { get; set; }

    [Option(Description = "Source IP address range for NSG inbound rules (e.g., '203.0.113.0/24' or a specific IP). Defaults to '*' (any source)")]
    public string? SourceAddressPrefix { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
