// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public sealed class TraceQueryRunOptions : ISubscriptionOption
{
    // Mirrors the trace query service's own query-structure guidance, with parameter names mapped to
    // this tool's options. Keep in sync when that guidance changes.
    private const string KqlQueryGuidance =
        "KQL query for the TraceStore three-part dialect. " +
        "STRUCTURE: <trace-filter> $$ <<span-filter>> $$ <<aggregation>> — include ONLY the parts you need; " +
        "do NOT add empty <<>> blocks, a bare 'Traces $$' prefix, or a bare `<<span-filter>>` with no leading " +
        "trace fragment (start with `Traces` or use `Traces $$ <<...>>` as a pass-through). Every fragment " +
        "(the trace part and each <<>> block) must start with 'Traces' and chain operators with '|'. " +
        "PLACEMENT: trace-level fields (e.g. duration_nano, span_count) go OUTSIDE <<>>; span-level fields " +
        "in `where` predicates go INSIDE <<>>. Reference dotted span fields with bracket syntax ['span.x.y'] — " +
        "never bare span.x.y and never joined with 'and'. " +
        "PROJECTING SPAN FIELDS: `<<>>` is for span-level `where` predicates ONLY — do NOT put `project ['span.x']` " +
        "inside `<<>>` (silently returns TRACE rows, not span rows). To project span fields, put the `project` " +
        "clause at the TOP LEVEL (outside `<<>>`) AND pass `--include-spans true`, e.g. " +
        "`Traces | where trace_id == '...' | project trace_id, ['span.name'], ['span.duration_nano']`. " +
        "Combine with a `<<span-filter>>` block via `$$` when you also need span-level predicates. " +
        "SAME vs DIFFERENT span: inside one <<>> block, '$$' separates conditions that must each match a DIFFERENT " +
        "span; to require several conditions on the SAME span, chain '| where' clauses in one fragment. " +
        "SPAN HIERARCHY (inside <<>>): `X ~> Y` means X is a strict ancestor of Y at any depth; `<~` reverses it. " +
        "They bind tighter than `$$` and chain left-to-right. Parentheses require a shared span: `X ~> (Y $$ Z)` " +
        "means the same X is above both; `(X ~> Y) $$ (X ~> Z)` may match different X spans. Prefer one complete " +
        "structural query over separate queries for each part of the relationship. " +
        "AGGREGATION: a summarize MUST be a separate SECOND <<>> block preceded by a span-filter <<>> block joined " +
        "with $$; a standalone <<Traces | summarize ...>> is treated as a span filter and returns rows, not " +
        "aggregates. If you have no span filter, use a pass-through <<Traces>> as the first block. " +
        "GOTCHAS: " +
        "`take` is NOT supported — use `--max-results` to limit rows; " +
        "comparison operators >= and <= are NOT supported (rewrite as > N-1 / < N+1); " +
        "negations: `!=`, `!in`, `!contains`, `!has` work; `!startswith` and `!endswith` do NOT — rewrite with `!contains` or negate the positive filter client-side; " +
        "ago()/now() in where are silently broken (use `--start-time`/`--end-time`); " +
        "supported aggregations (definitive): count, sum, min, max, percentile — anything else needs a workaround; " +
        "`summarize ... by bin()` using bin() function silently returns null; for histograms, fetch raw values with a top-level project of the span field (one query) and bucket client-side — do NOT emit one query per bucket; bare `summarize <aggregation>()` without a `by` clause fails; " +
        "`order by` inside a `<<>>` aggregation block is silently dropped — omit it and sort client-side after the response; " +
        "timespan literals like 1s are silently null — use explicit nanoseconds; " +
        "matches regex returns arbitrary rows — use startswith/endswith/contains; " +
        "per-query time-range is capped at 1 day. " +
        "EXAMPLES: " +
        "'Traces' (all traces); " +
        "'Traces | where duration_nano > 10' (trace filter); " +
        "'Traces | where span_count in (2, 3)' (trace filter, multiple values); " +
        "'<<Traces | where ['span.name'] == 'GET'>>' (span filter); " +
        "'<<Traces | where ['span.name'] == 'GET' | where ['span.status.code'] == 'Error'>>' (SAME span, chained where); " +
        "'<<Traces | where ['span.name'] in ('GET', 'BlobClient.Upload')>>' (span filter, multiple values); " +
        "'Traces | where duration_nano > 10 $$ <<Traces | where ['span.name'] != 'GET' $$ Traces | where ['span.name'] == 'BlobClient.Upload'>>' (trace + two DIFFERENT spans); " +
        "'<<Traces | where ['span.service.namespace'] == 'my-ns'>> $$ <<Traces | summarize count=count() by ['span.service.namespace']>>' (aggregation with span filter); " +
        "'<<Traces>> $$ <<Traces | summarize count=count() by ['span.service.name']>>' (aggregation, no filter); " +
        "'Traces | where duration_nano > 10 $$ <<Traces | where ['span.name'] == 'GET'>> $$ <<Traces | summarize sum=sum(duration_nano) by ['span.name']>>' (all three parts).";

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the Azure Monitor Workspace enabled for Trace Store.")]
    public required string Workspace { get; set; }

    [Option(Description = KqlQueryGuidance)]
    public required string Query { get; set; }

    [Option(Description = "Query start time in UTC ISO 8601 (e.g. 2026-07-24T20:00:00Z). Defaults to 30 minutes before end-time.")]
    public string? StartTime { get; set; }

    [Option(Description = "Query end time in UTC ISO 8601. Defaults to the current UTC time.")]
    public string? EndTime { get; set; }

    [Option(Description = "Total rows requested for the new query and collected per call. Default 100; clamped to 1-1000. Whole backend pages may exceed the request.")]
    public int? MaxResults { get; set; }

    [Option(Description = "Include span-level detail in the response. Default true.")]
    public bool? IncludeSpans { get; set; }
}
