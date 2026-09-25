// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Optimization.Services;

/// <summary>English markdown/text templates for the optimization tools.</summary>
internal static class OptimizationStrings
{
    // ---- Recommendation explanation ----
    public const string ErrorInvalidResourceId =
        "Invalid Azure resource ID format. Ensure it starts with '/subscriptions/' and is a valid ARM ID.";

    // Delivered inside the response payload to guide how the agent presents the utilization data.
    public const string ExplanationRenderingInstructions =
        "When possible, render the recentUtilization (and longTermUtilization when present) series as an inline " +
        "line/time-series chart, with the timestamp on the x-axis and percentage on the y-axis. Draw separate lines for " +
        "current versus target CPU and used-memory utilization; include network utilization only when network values are " +
        "present (it may be absent). Mark the threshold levels from thresholds when available. Prefer a native inline " +
        "chart/visualization capability if one is available. After the chart, briefly summarize the recommendation, the " +
        "current-versus-target configuration, the max-utilization comparison, and any threshold risks. If inline chart " +
        "rendering is not available, summarize the data in text instead.";

    // Shared
    public const string NotAvailableValue = "N/A";
    public const string PercentageFormat = "{0}%";

    // Subscription option: accepts a name or id and resolves the id internally via Azure Resource Graph,
    // so the agent must not call the separate 'subscription list' tool first.
    public const string SubscriptionOptionDescription =
        "Azure subscription id (GUID) or subscription name. Pass whatever the user gives you directly \u2014 a name is " +
        "resolved to its id internally with a single targeted Azure Resource Graph lookup. Do NOT call the 'subscription " +
        "list' tool (or any list-subscriptions tool) to resolve the id first.";

    // ---- Alternative recommendations ----
    public const string AltInvalidResourceIdMessage = "Invalid resourceId.";
    public const string AltHeader = "## Alternative Cost Saving Recommendations";
    public const string AltResourceIdLabel = "**Resource ID**: `{0}`";
    public const string AltNoRecommendationsFoundMessage = "No alternative recommendations found";
    public const string AltNoRecommendationsForFiltersSuffix = " for filters ({0}).";
    public const string AltNoRecommendationsPeriod = ".";
    public const string AltPossibleReasonsHeader = "Possible reasons:";
    public const string AltPossibleReasonOne = "- No viable alternative recommendations identified for cost/performance";
    public const string AltPossibleReasonTwo = "- Filters too restrictive";
    public const string AltPossibleReasonThree = "- Data not yet available";
    public const string AltTryAdjustFiltersMessage = "Try adjusting or removing some filters.";
    public const string AltAppliedProposedFiltersLabel = "**Applied Proposed Filters**: {0}";
    public const string AltReturnedLabel = "**Returned**: {0}";
    public const string AltObservationWindowLabel = "**Observation Window**: ({0} days)";
    public const string AltTableHeader =
        "| Option | RecommendationMessage | Proposed SKU | Proposed Series | Proposed Processor | Estimated Monthly Savings | Estimated Core Savings |\n|--------|-----------------------|--------------|-----------------|--------------------|---------------------------|------------------------|";
    public const string AltRow = "| {0} | **{1}** | {2} | {3} | {4} | {5:F0} {6} | {7} |";
    public const string AltSummary =
        "Alternative recommendations are derived from an observation window of {0} days and are ranked based on their potential for cost savings, ease of implementation, and anticipated impact on performance.";

    public const string FilterSkusLabel = "SKUs: {0}";
    public const string FilterSeriesLabel = "Series: {0}";
    public const string FilterProcessorsLabel = "Processors: {0}";
    public const string FilterExcludeSkusLabel = "Exclude SKUs: {0}";
    public const string FilterExcludeSeriesLabel = "Exclude Series: {0}";
    public const string FilterExcludeProcessorsLabel = "Exclude Processors: {0}";
    public const string FilterJoinSeparator = ";";
}
