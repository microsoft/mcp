// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Options;

public static class AksOptionDefinitions
{
    public const string ClusterName = "cluster";
    public const string NodepoolName = "nodepool";

    public static readonly Option<string> Cluster = new($"--{ClusterName}")
    {
        Description = "AKS Cluster name.",
    };

    public static readonly Option<string> Nodepool = new($"--{NodepoolName}")
    {
        Description = "AKS node pool (agent pool) name.",
    };
}
