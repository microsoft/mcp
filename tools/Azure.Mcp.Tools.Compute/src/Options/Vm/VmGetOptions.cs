// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public sealed class VmGetOptions : ISubscriptionOption
{
    [Option(Description = ComputeOptionDescriptions.VmName, Aliases = ["name"])]
    public string? VmName { get; set; }

    [Option(Description = "Include instance view details (only available when retrieving a specific VM)")]
    public bool InstanceView { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
