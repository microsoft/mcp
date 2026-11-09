// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public sealed class TraceAnalyzeCriticalPathOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the Azure Monitor Workspace enabled for Trace Store.")]
    public required string Workspace { get; set; }

    [Option(Description = "1-1000 trace IDs from monitor traces query run. Each must be 32 hexadecimal characters or a GUID.")]
    public required string[] TraceIds { get; set; }

    [Option(Description = "Query start time in UTC ISO 8601. The window must cover the traces and be positive and at most 24 hours.")]
    public required DateTimeOffset StartTime { get; set; }

    [Option(Description = "Query end time in UTC ISO 8601.")]
    public required DateTimeOffset EndTime { get; set; }

    [Option(Description = "Up to 3 span fields or custom attribute names to group by, for example span.service.name or span.attributes.guid:x-request-id. Default span.name. No brackets or quotes.")]
    public string[]? AggregateBy { get; set; }

    [Option(Description = "Up to 5 percentiles, each 1-99. Defaults to 50 95 99.")]
    public int[]? Percentiles { get; set; }

    [Option(Description = "Span-name prefixes whose subtrees are removed entirely before critical-path scoring.")]
    public string[]? ExcludeSubtreePrefixes { get; set; }

    [Option(Description = "Span-name prefixes whose subtrees are demoted, not removed; they contribute only when no higher-priority work runs concurrently.")]
    public string[]? DeprioritizeSubtreePrefixes { get; set; }

    [Option(Description = "Follow OpenTelemetry span links across trace boundaries. Default false.")]
    public bool? ResolveTraceLinks { get; set; }

    [Option(Description = "Include sample trace/span IDs for each percentile. Default true.")]
    public bool? IncludeSamples { get; set; }

    [Option(Description = "Include per-trace critical-path totals in the backend metadata. Default false.")]
    public bool? IncludeRollups { get; set; }

    [Option(Description = "Requested result-row limit. Default 10; clamped to 1-1000. Whole result pages may exceed it.")]
    public int? MaxResults { get; set; }
}
