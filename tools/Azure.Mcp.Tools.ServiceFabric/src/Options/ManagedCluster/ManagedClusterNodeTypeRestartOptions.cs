// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ServiceFabric.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ServiceFabric.Options.ManagedCluster;

public sealed class ManagedClusterNodeTypeRestartOptions : ISubscriptionOption
{
    [Option(Description = ServiceFabricOptionDescriptions.ClusterDescription)]
    public required string Cluster { get; set; }

    [Option(Description = "The node type name within the managed cluster.")]
    public required string NodeType { get; set; }

    [Option(Description = "The list of node names to restart. Multiple node names can be provided.")]
    public required string[] Nodes { get; set; }

    [Option(Description = "The update type for the restart operation.")]
    public UpdateType? UpdateType { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
