// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public sealed class TraceCompareCriticalPathOptions()
    : ITraceCompareOptions
{
    [Option(Description = "Current population KQL filter query, instead of --trace-ids.")]
    public string? Query { get; set; }

    [Option(Description = "Current population start time, UTC ISO 8601. Each window must be 1 minute to 1 day and cannot start in the future.")]
    public required string StartTime { get; set; }

    [Option(Description = "Current population end time, UTC ISO 8601.")]
    public required string EndTime { get; set; }

    [Option(Description = "Comparison population KQL filter query, instead of --compare-trace-ids.")]
    public string? CompareQuery { get; set; }

    [Option(Description = "Comparison start time. Supply both comparison timestamps or neither; omission inherits the current window.")]
    public string? CompareStartTime { get; set; }

    [Option(Description = "Comparison end time, UTC ISO 8601.")]
    public string? CompareEndTime { get; set; }

    [Option(Description = "Maximum distinct trace IDs per population, default 10000, clamped to 1..10000. The backend separately allows at most 1000 IDs per population. Exceeding either limit fails rather than sampling.")]
    public int? MaxResults { get; set; }

    [Option(Description = "Critical-path only: 1..1000 explicit current trace IDs (32 hexadecimal characters or GUIDs), instead of --query.")]
    public string[]? TraceIds { get; set; }

    [Option(Description = "Critical-path only: 1..1000 explicit comparison trace IDs, instead of --compare-query.")]
    public string[]? CompareTraceIds { get; set; }

    [Option(Description = "Critical-path only: grouping fields. Default span.name; maximum 3. Accepts bare or bracket-quoted field names.")]
    public string[]? AggregateBy { get; set; }

    [Option(Description = "Critical-path only: up to 5 distinct percentiles, each 1..99. Default 50, 95, 99.")]
    public int[]? Percentiles { get; set; }

    [Option(Description = "Critical-path only: resolve linked traces. Default false.")]
    public bool? ResolveTraceLinks { get; set; }

    [Option(Description = "Critical-path only: include sample trace IDs per aggregation row. Default true.")]
    public bool? IncludeSamples { get; set; }

    [Option(Description = "Critical-path only: include roll-up rows. Default false.")]
    public bool? IncludeRollups { get; set; }

    [Option(Description = "Critical-path only: span-name prefixes excluded with their subtrees.")]
    public string[]? ExcludeSubtreePrefixes { get; set; }

    [Option(Description = "Critical-path only: span-name prefixes given lower priority when concurrent work exists.")]
    public string[]? DeprioritizeSubtreePrefixes { get; set; }

    [Option(Description = "The Azure Monitor Workspace resource name enabled for Trace Store.")]
    public required string Workspace { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
