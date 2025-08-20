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
        $"--{ClusterName}",
        "Kusto Cluster name."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> ClusterUri = new(
        $"--{ClusterUriName}",
        "Kusto Cluster URI."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}",
        "Kusto Database name."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Table = new(
        $"--{TableName}",
        "Kusto Table name."
    )
    {
        IsRequired = true
    };

    public static readonly Option<int> Limit = new(
        $"--{LimitName}",
        () => 10,
        "The maximum number of results to return."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryText}",
        "Kusto query to execute. Uses KQL syntax."
    )
    {
        IsRequired = true
    };
}
