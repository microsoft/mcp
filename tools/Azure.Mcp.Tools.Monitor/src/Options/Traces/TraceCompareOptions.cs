// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public sealed class TraceCompareOptions()
    : ITraceCompareOptions
{
    [Option(Description = "Required current population KQL. For statistics, use matching named aggregations and grouping keys in both queries.")]
    public required string Query { get; set; }

    [Option(Description = "Current population start time, UTC ISO 8601. Each window must be 1 minute to 1 day and cannot start in the future.")]
    public required string StartTime { get; set; }

    [Option(Description = "Current population end time, UTC ISO 8601.")]
    public required string EndTime { get; set; }

    [Option(Description = "Independent comparison KQL. For statistics and attributes, omit to reuse the current query over --compare-start-time/--compare-end-time.")]
    public string? CompareQuery { get; set; }

    [Option(Description = "Comparison start time. Supply both comparison timestamps or neither; omission inherits the current window.")]
    public string? CompareStartTime { get; set; }

    [Option(Description = "Comparison end time, UTC ISO 8601.")]
    public string? CompareEndTime { get; set; }

    [Option(Description = "Per-population backend query limit, default 10000, clamped to 1..10000. For statistics this limits aggregation rows, not input traces; for attributes it limits fetched traces. Limits may be approximate.")]
    public int? MaxResults { get; set; }

    [Option(Description = "The Azure Monitor Workspace resource name enabled for Trace Store.")]
    public required string Workspace { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
