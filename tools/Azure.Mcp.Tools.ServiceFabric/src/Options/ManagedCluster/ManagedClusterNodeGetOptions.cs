// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceFabric.Options.ManagedCluster;

public sealed class ManagedClusterNodeGetOptions : ISubscriptionOption
{
    [Option(Description = ServiceFabricOptionDescriptions.ClusterDescription)]
    public required string Cluster { get; set; }

    [Option(Description = "The node name. When specified, returns a single node instead of all nodes.")]
    public string? Node { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
