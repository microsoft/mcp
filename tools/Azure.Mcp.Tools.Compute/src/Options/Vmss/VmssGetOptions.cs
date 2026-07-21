// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public sealed class VmssGetOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmssName, Aliases = ["name"])]
    public string? VmssName { get; set; }

    [Option(Description = "The instance ID of the virtual machine in the scale set")]
    public string? InstanceId { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
