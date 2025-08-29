// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Options;

public static class MonitorOptionDefinitions
{
    public const string TableNameName = "table";
    public const string TableTypeName = "table-type";
    public const string QueryTextName = "query";
    public const string HoursName = "hours";
    public const string LimitName = "limit";
    public const string EntityName = "entity";
    public const string HealthModelName = "health-model";

    public static readonly Option<string> TableType = new(
        $"--{TableTypeName}"
    )
    {
        Description = "The type of table to query. Options: 'CustomLog', 'AzureMetrics', etc.",
        DefaultValueFactory = _ => "CustomLog",
        Required = true
    };

    public static readonly Option<string> TableName = new(
        $"--{TableNameName}"
    )
    {
        Description = "The name of the table to query. This is the specific table within the workspace.",
        Required = true
    };

    public static readonly Option<string> Query = new(
        $"--{QueryTextName}"
    )
    {
        Description = "The KQL query to execute against the Log Analytics workspace. You can use predefined queries by name:\n" +
                      "- 'recent': Shows most recent logs ordered by TimeGenerated\n" +
                      "- 'errors': Shows error-level logs ordered by TimeGenerated\n" +
                      "Otherwise, provide a custom KQL query.",
        Required = true
    };

    public static readonly Option<int> Hours = new(
        $"--{HoursName}"
    )
    {
        Description = "The number of hours to query back from now.",
        DefaultValueFactory = _ => 24,
        Required = false
    };

    public static readonly Option<int> Limit = new(
        $"--{LimitName}"
    )
    {
        Description = "The maximum number of results to return.",
        DefaultValueFactory = _ => 20,
        Required = false
    };

    public static class Metrics
    {
        // Metrics related options
        public const string ResourceIdName = "resource-id";
        public const string ResourceTypeName = "resource-type";
        public const string ResourceNameName = "resource";
        public const string MetricNamespaceName = "metric-namespace";
        public const string MetricNamesName = "metric-names";
        public const string StartTimeName = "start-time";
        public const string EndTimeName = "end-time";
        public const string IntervalName = "interval";
        public const string AggregationName = "aggregation";
        public const string FilterName = "filter";
        public const string SearchStringName = "search-string";

        public const string EntityName = "entity";
        public const string HealthModelName = "health-model";
        public const string MaxBucketsName = "max-buckets";

        // Metrics options
        public static readonly Option<string> MetricNamespaceOptional = new(
            $"--{MetricNamespaceName}"
        )
        {
            Description = "The metric namespace to query. Obtain this value from the azmcp-monitor-metrics-definitions command.",
            Required = false
        };

        public static readonly Option<string> MetricNamespace = new(
            $"--{MetricNamespaceName}"
        )
        {
            Description = "The metric namespace to query. Obtain this value from the azmcp-monitor-metrics-definitions command.",
            Required = true
        };

        public static readonly Option<string> MetricNames = new(
            $"--{MetricNamesName}"
        )
        {
            Description = "The names of metrics to query (comma-separated).",
            Required = true,
            AllowMultipleArgumentsPerToken = true
        };

        public static readonly Option<string> StartTime = new(
            $"--{StartTimeName}"
        )
        {
            Description = "The start time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to 24 hours ago.",
            DefaultValueFactory = _ => DateTime.UtcNow.AddHours(-24).ToString("o")
        };

        public static readonly Option<string> EndTime = new(
            $"--{EndTimeName}"
        )
        {
            Description = "The end time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to now.",
            DefaultValueFactory = _ => DateTime.UtcNow.ToString("o")
        };

        public static readonly Option<string> Interval = new(
            $"--{IntervalName}"
        )
        {
            Description = "The time interval for data points (e.g., PT1H for 1 hour, PT5M for 5 minutes)."
        };

        public static readonly Option<string> Aggregation = new(
            $"--{AggregationName}"
        )
        {
            Description = "The aggregation type to use (Average, Maximum, Minimum, Total, Count)."
        };

        public static readonly Option<string> Filter = new(
            $"--{FilterName}"
        )
        {
            Description = "OData filter to apply to the metrics query."
        };

        public static readonly Option<string> SearchString = new(
            $"--{SearchStringName}"
        )
        {
            Description = "A string to filter the metric definitions by. Helpful for reducing the number of records returned. Performs case-insensitive matching on metric name and description fields.",
            Required = false
        };

        public static readonly Option<int> DefinitionsLimit = new(
            $"--limit"
        )
        {
            Description = "The maximum number of metric definitions to return. Defaults to 10.",
            DefaultValueFactory = _ => 10,
            Required = false
        };

        public static readonly Option<int> NamespacesLimit = new(
            $"--limit"
        )
        {
            Description = "The maximum number of metric namespaces to return. Defaults to 10.",
            DefaultValueFactory = _ => 10,
            Required = false
        };

        public static readonly Option<int> MaxBuckets = new(
            $"--{MaxBucketsName}"
        )
        {
            Description = "The maximum number of time buckets to return. Defaults to 50.",
            DefaultValueFactory = _ => 50,
            Required = false
        };


        public static readonly Option<string> ResourceType = new(
            $"--{ResourceTypeName}"
        )
        {
            Description = "The Azure resource type (e.g., 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines'). If not specified, will attempt to infer from resource name.",
            Required = false
        };

        public static readonly Option<string> ResourceName = new(
            $"--{ResourceNameName}"
        )
        {
            Description = "The name of the Azure resource to query metrics for.",
            Required = true
        };
    }

    public static class Health
    {
        public static readonly Option<string> Entity = new(
            $"--{EntityName}"
        )
        {
            Description = "The entity to get health for.",
            Required = true
        };

        public static readonly Option<string> HealthModel = new(
            $"--{HealthModelName}"
        )
        {
            Description = "The name of the health model for which to get the health.",
            Required = true
        };
    }
}
