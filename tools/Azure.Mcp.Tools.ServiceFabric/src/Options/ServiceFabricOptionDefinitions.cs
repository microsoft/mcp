// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ServiceFabric.Options;

public static class ServiceFabricOptionDefinitions
{
    public const string ClusterName = "cluster";
    public const string NodeName = "node";
    public const string NodeTypeName = "node-type";
    public const string NodesName = "nodes";
    public const string UpdateTypeName = "update-type";

    public static readonly Option<string> Cluster = new($"--{ClusterName}")
    {
        Description = "Service Fabric managed cluster name.",
    };

    public static readonly Option<string> Node = new($"--{NodeName}")
    {
        Description = "The node name. When specified, returns a single node instead of all nodes.",
    };

    public static readonly Option<string> NodeType = new($"--{NodeTypeName}")
    {
        Description = "The node type name within the managed cluster.",
    };

    public static readonly Option<string[]> Nodes = new($"--{NodesName}")
    {
        Description = "The list of node names to restart. Multiple node names can be provided.",
        Arity = ArgumentArity.OneOrMore,
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string> UpdateType = CreateUpdateTypeOption();

    private static Option<string> CreateUpdateTypeOption()
    {
        var option = new Option<string>($"--{UpdateTypeName}")
        {
            Description = "The update type for the restart operation. Valid values: Default, ByUpgradeDomain.",
        };
        option.AcceptOnlyFromAmong("Default", "ByUpgradeDomain");
        return option;
    }
}
