
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;

public class BaseHostPoolOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure Virtual Desktop host pool. This is the unique name you chose for your hostpool.")]
    public string? Hostpool { get; set; }

    [Option(Description = "The Azure resource ID of the host pool. When provided, this will be used instead of searching by name.")]
    public string? HostpoolResourceId { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
