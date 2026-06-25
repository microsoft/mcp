// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public sealed class VmssUpdateOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmssName, Aliases = ["name"])]
    public required string VmssName { get; set; }

    [Option(Description = ComputeOptionDescriptions.VmSize, Aliases = ["size"])]
    public string? VmSize { get; set; }

    [Option(Description = "Number of VM instances (capacity) in the scale set")]
    public int? Capacity { get; set; }

    [Option(Description = ComputeOptionDescriptions.UpgradePolicy)]
    public string? UpgradePolicy { get; set; }

    [Option(Description = "Enable or disable overprovisioning. When enabled, Azure provisions more VMs than requested and deletes extra VMs after deployment")]
    public bool? Overprovision { get; set; }

    [Option(Description = "Enable automatic OS image upgrades. Requires health probes or Application Health extension")]
    public bool? EnableAutoOsUpgrade { get; set; }

    [Option(Description = "Enable or disable overprovisioning. When enabled, Azure provisions more VMs than requested and deletes extra VMs after deployment")]
    public string? ScaleInPolicy { get; set; }

    [Option(Description = ComputeOptionDescriptions.TagsUpdate, AllowEmptyOrWhiteSpaceString = true)]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
