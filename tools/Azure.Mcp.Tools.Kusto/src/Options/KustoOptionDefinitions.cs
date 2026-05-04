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
        Description = "When specified, the query results are rendered as a chart image and returned as the MCP response (the JSON results are omitted), enabling vision-capable LLMs to analyze the data visually. " +
                      "Valid values: TimeSeries (requires a datetime column and at least one numeric column), " +
                      "Bar (requires a string/label column and a numeric column), " +
                      "Scatter (requires two numeric columns), " +
                      "Pie (requires a string/label column and a numeric column). " +
                      "If the data shape does not match the requested chart type, the tool call fails with an explanation.",
        Required = false
    };

    /// <summary>
    /// Registers a command-level validator that fails the parse if the supplied <c>--chart-type</c>
    /// value cannot be mapped to a <see cref="Rendering.ChartType"/>. Call from
    /// <c>BaseCommand.RegisterOptions</c> after <see cref="ChartType"/> has been added.
    /// </summary>
    public static void AddChartTypeValidator(Command command)
    {
        command.Validators.Add(commandResult =>
        {
            var optionResult = commandResult.GetResult(ChartType);
            if (optionResult is null)
            {
                return;
            }

            var value = optionResult.GetValueOrDefault<string?>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!Enum.TryParse<Rendering.ChartType>(value, ignoreCase: true, out _))
            {
                var allowed = string.Join(", ", Enum.GetNames<Rendering.ChartType>());
                commandResult.AddError($"Invalid value '{value}' for --{ChartTypeName}. Allowed values: {allowed}.");
            }
        });
    }
}
