// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Kusto.Options;

public static class KustoOptionDefinitions
{
    public const string ClusterName = "cluster";
    public const string ClusterUriName = "cluster-uri";
    public const string DatabaseName = "database";
    public const string TableName = "table";
    public const string LimitName = "limit";
    public const string QueryText = "query";


    public static readonly Option<string> Cluster = new(
        $"--{ClusterName}"
    )
    {
        Description = "Kusto Cluster name.",
        Required = false
    };

    public static readonly Option<string> ClusterUri = new(
        $"--{ClusterUriName}"
    )
    {
        Description = "Kusto Cluster URI.",
        Required = false
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}"
    )
    {
        Description = "Kusto Database name.",
        Required = true
    };

    public static readonly Option<string> Table = new(
        $"--{TableName}"
    )
    {
        Description = "Kusto Table name.",
        Required = true
    };

    public static readonly Option<int> Limit = new(
        $"--{LimitName}"
    )
    {
        Description = "The maximum number of results to return.",
        DefaultValueFactory = _ => 10,
        Required = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryText}"
    )
    {
        Description = "Kusto query to execute. Uses KQL syntax.",
        Required = true
    };
}
