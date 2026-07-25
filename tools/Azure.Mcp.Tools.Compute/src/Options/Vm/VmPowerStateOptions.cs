// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public sealed class VmPowerStateOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmName, Aliases = ["name"])]
    public required string VmName { get; set; }

    [Option(Description = "The power action to apply to the VM (not the current power state). Accepted values: start, stop, deallocate, restart.")]
    public required string PowerAction { get; set; }

    [Option(Description = "Return immediately without waiting for the operation to complete.")]
    public bool NoWait { get; set; }

    [Option(Description = "Skip the graceful OS shutdown and force power off. Only compatible with the 'stop' state.")]
    public bool SkipShutdown { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
