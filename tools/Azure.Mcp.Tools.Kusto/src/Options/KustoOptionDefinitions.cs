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
    public const string ChartTypeName = "chart-type";


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
        Description = "The maximum number of results to return. Must be a positive integer between 1 and 10000. Default is 10.",
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

    public static readonly Option<string?> ChartType = new(
        $"--{ChartTypeName}"
    )
    {
        Description = "When specified, renders the query results as a chart image and includes it as an image content block in the MCP response, enabling LLMs with vision capability to analyze the data visually. " +
                      "Valid values: TimeSeries (requires a datetime column and at least one numeric column), " +
                      "Bar (requires a string/label column and a numeric column), " +
                      "Scatter (requires two numeric columns), " +
                      "Pie (requires a string/label column and a numeric column). " +
                      "If the data shape does not match the requested chart type the image is omitted and only the JSON results are returned.",
        Required = false
    };
}
