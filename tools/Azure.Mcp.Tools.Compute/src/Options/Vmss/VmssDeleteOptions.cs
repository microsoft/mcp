// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public sealed class VmssDeleteOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmssName, Aliases = ["name"])]
    public required string VmssName { get; set; }

    [Option(Description = ComputeOptionDescriptions.ForceDeletion)]
    public bool ForceDeletion { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
