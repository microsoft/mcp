// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public sealed class VmssCreateOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmssName, Aliases = ["name"])]
    public required string VmssName { get; set; }

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

    [Option(Description = "Number of VM instances in the scale set. Default is 2", DefaultValue = 2)]
    public int? InstanceCount { get; set; }

    [Option(Description = ComputeOptionDescriptions.UpgradePolicy + " Default is 'Manual'", DefaultValue = "Manual")]
    public string? UpgradePolicy { get; set; }

    [Option(Description = ComputeOptionDescriptions.Zone)]
    public string? Zone { get; set; }

    [Option(Description = ComputeOptionDescriptions.OsDiskSizeGb)]
    public int? OsDiskSizeGb { get; set; }

    [Option(Description = ComputeOptionDescriptions.OsDiskType)]
    public string? OsDiskType { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
