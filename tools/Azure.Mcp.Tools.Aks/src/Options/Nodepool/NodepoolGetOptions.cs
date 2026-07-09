// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Aks.Options.Nodepool;

public class NodepoolGetOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }

    [Option(Description = "AKS Cluster name.")]
    public required string Cluster { get; set; }

    [Option(Description = "AKS node pool (agent pool) name.")]
    public string? Nodepool { get; set; }
}

