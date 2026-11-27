// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Traces;

public sealed class TraceAnalyzeTopAttributesOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the Azure Monitor Workspace enabled for Trace Store.")]
    public required string Workspace { get; set; }

    [Option(Description = "Trace Store KQL defining the overall pool of traces. Defaults to Traces. Use monitor traces query run for dialect guidance.")]
    public string? OverallQuery { get; set; }

    [Option(Description = "Overall pool start time in UTC ISO 8601. The overall window must be positive and at most 24 hours.")]
    public required DateTimeOffset OverallStartTime { get; set; }

    [Option(Description = "Overall pool end time in UTC ISO 8601.")]
    public required DateTimeOffset OverallEndTime { get; set; }

    [Option(Description = "Selection start time in UTC ISO 8601. The selection window must lie within the overall window.")]
    public required DateTimeOffset SelectionStartTime { get; set; }

    [Option(Description = "Selection end time in UTC ISO 8601.")]
    public required DateTimeOffset SelectionEndTime { get; set; }

    [Option(Description = "Bare selection field: duration_nano, error_rate, span_count, or a span attribute key. Do not bracket-quote.")]
    public required string SelectionField { get; set; }

    [Option(Description = "Selection comparison operator: >, >=, <, <=, ==, or !=.")]
    public required string SelectionOp { get; set; }

    [Option(Description = "Selection value as a finite number or single-quoted string literal, for example 1000 or 'fail'.")]
    public required string SelectionValue { get; set; }

    [Option(Description = "Top distinguishing values per attribute. Default 8; range 1-1000.")]
    public int? TopN { get; set; }

    [Option(Description = "Maximum number of traces requested for the backend comparison sample. Default 10000; clamped to 1-10000. Smaller samples can change the ranking; top-n limits values per attribute, not traces.")]
    public int? MaxResults { get; set; }
}
