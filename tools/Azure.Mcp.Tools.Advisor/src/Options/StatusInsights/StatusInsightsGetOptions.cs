// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.StatusInsights;

public sealed class StatusInsightsGetOptions
{
    [Option(Description = "Optional Service Group names or full resource IDs " +
        "(/providers/Microsoft.Management/serviceGroups/{name}). Omit to query all Service Groups visible to the caller.")]
    public string[]? ServiceGroup { get; set; }

    [Option(Description = "Optional criticality tiers to keep: 0 Mission-critical, 1 Business-critical, 2 Business-operational, 3 Administrative.")]
    public string[]? CriticalityTier { get; set; }

    [Option(Description = "Optional Status values to keep: Critical, At Risk, Attention Needed, On Track, Unavailable.")]
    public string[]? Status { get; set; }

    [Option(Description = "Optional Insight names to include, such as ZonalResiliency, IdleResources, or ServiceRetirement. Omit for all.")]
    public string[]? InsightName { get; set; }

    [Option(Description = "What to return: 'status' (Status, StatusDescription, Description and criticality only; use when the user asks only for Status, health or criticality), 'insights' (Insight KPIs only; use when the user asks only for Insights such as zonal resiliency, idle resources or service retirement) or 'both' (full report, or when unsure).",
        DefaultValue = "both")]
    public string Include { get; set; } = "both";

    [Option(Description = "Optional continuation token returned by a previous partial result.")]
    public string? ContinuationToken { get; set; }

}
