// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Options;

public static class AksOptionDefinitions
{
    public const string ClusterName = "cluster";

    public static readonly Option<string> Cluster = new(
        $"--{ClusterName}",
        "AKS Cluster name."
    )
    {
        IsRequired = true
    };
}
