// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Metrics;

/// <summary>
/// Options for querying metrics for multiple resources in a single batch request
/// </summary>
public sealed class MetricsBatchQueryOptions : ISubscriptionOption
{
    /// <summary>
    /// The resources to query metrics for (required)
    /// </summary>
    [Option(Description = "Comma-separated list of resource names or full resource IDs to query metrics for (up to 50 resources). " +
        "All resources must belong to the same subscription, Azure region, and resource type.")]
    public required string Resources { get; set; }

    /// <summary>
    /// The resource type (optional, e.g., 'Microsoft.Storage/storageAccounts')
    /// </summary>
    [Option(Description = "The Azure resource type (e.g., 'Microsoft.Storage/storageAccounts', 'Microsoft.Compute/virtualMachines'), applied to all resources. If not specified, will attempt to infer from each resource name.")]
    public string? ResourceType { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    /// <summary>
    /// The names of metrics to query
    /// </summary>
    [Option(Description = "The names of metrics to query (comma-separated).")]
    public required string MetricNames { get; set; }

    [Option(Description = MonitorOptionDescriptions.MetricNamespace)]
    public required string MetricNamespace { get; set; }

    /// <summary>
    /// Start time for the query in ISO format
    /// </summary>
    [Option(Description = "The start time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to 24 hours ago.")]
    public string? StartTime { get; set; }

    /// <summary>
    /// End time for the query in ISO format
    /// </summary>
    [Option(Description = "The end time for the query in ISO format (e.g., 2023-01-01T00:00:00Z). Defaults to now.")]
    public string? EndTime { get; set; }

    /// <summary>
    /// Time interval for the query
    /// </summary>
    [Option(Description = "The time interval for data points (e.g., PT1H for 1 hour, PT5M for 5 minutes).")]
    public string? Interval { get; set; }

    /// <summary>
    /// Aggregation type(s) for the metrics (Average, Maximum, Minimum, Total, Count)
    /// </summary>
    [Option(Description = "The aggregation type(s) to use (comma-separated, e.g., Average,Maximum).")]
    public string? Aggregation { get; set; }

    /// <summary>
    /// OData filter for the query
    /// </summary>
    [Option(Description = "OData filter to apply to the metrics query.")]
    public string? Filter { get; set; }

    /// <summary>
    /// The aggregation to use for sorting results and the direction of the sort. Only valid when '--filter' is specified.
    /// </summary>
    [Option(Description = "The aggregation to use for sorting results and the direction of the sort (e.g., 'total asc'). Only valid when '--filter' is specified.")]
    public string? OrderBy { get; set; }

    /// <summary>
    /// The maximum number of records to retrieve per resource. Only valid when '--filter' is specified.
    /// </summary>
    [Option(Description = "The maximum number of time series to retrieve per resource per metric. Only valid when '--filter' is specified. Defaults to 10.")]
    public int? Top { get; set; }

    /// <summary>
    /// The maximum number of time buckets to return per metric time series. Defaults to 50.
    /// </summary>
    [Option(Description = "The maximum number of time buckets to return per metric time series. Defaults to 50.", DefaultValue = 50)]
    public int? MaxBuckets { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
