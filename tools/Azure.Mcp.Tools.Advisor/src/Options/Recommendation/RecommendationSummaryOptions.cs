// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

/// <summary>Options for aggregating Advisor recommendations into per-bucket counts.</summary>
public sealed class RecommendationSummaryOptions : IRecommendationReadScopeOptions
{
    [Option(Description = "The name, ID, or full ARM resource ID of the Azure Service Group whose recommendations should be summarized. The tool resolves the scope in the selected tenant and reports ambiguous names. Specify either --service-group or --subscription. If both are omitted, the configured default subscription is used.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "Optional field to group the summary by. One of: 'recommendation-type', 'category', 'impact', 'resource-type', 'status', 'sub-category', or 'retirement-date'. " +
        "Defaults to 'category' when omitted, which surfaces the high-level themes (Cost, Security, Reliability, etc.) " +
        "so prompts like 'summarize the key themes from my Advisor recommendations' work without naming a field. " +
        "Use 'recommendation-type' without --prioritized for frequency questions such as the most common recommendation types; its counts are recommendation records. " +
        "With --prioritized true, omit this option; prioritized summaries are always grouped by recommendation type.")]
    public string? GroupBy { get; set; }

    [Option(Description = "Rank matching recommendation types by priority instead of counting records. Defaults to false; recommendations default to New, or Completed with historical inputs. An explicit status overrides these defaults. " +
        "Set to true for top recommendations, top actions to take, most critical, most important, highest-priority, or what-should-I-fix-first requests, overall or within filters such as category Cost; " +
        "pass a requested count as --top (default 10). Do not set it for frequency questions such as how many or most common. Groups by recommendation-type. " +
        "Cost with subcategory IdleResources ranks savings first, then metadata priority; other workflows rank metadata priority first, then savings. Savings means annual potential savings, or realized savings for completed Cost recommendations. Remaining tie-breakers are summed criticality score, earliest retirement date, and affected-resource count. " +
        "Each group's count is its distinct affected resources, and totalRecommendations is the sum of those counts across all ranked types, not a record count; each group's prioritizationSignals object can hold category, impact, subCategory, retirementDate, retirementWindow, annualSavings with USD currency, yearlyPotentialSavings, cost, realizedSavings, " +
        "resourceTypeCriticality, criticalService, effort, actionability, and costImplication; absent fields are unknown, never zero. " +
        "To explain a ranking, answer from prioritizationSignals in two or three plain business sentences, leading with impact, then retirementWindow or subCategory, then the other signals; " +
        "translate values (retirementPastDue: retirement date has passed; qcs: critical service) and never mention field names, raw values, or scores, or guess absent attributes. " +
        "For details or affected resources of a listed type, call recommendation list with --recommendation-type-id set to its key and --prioritized true, keeping the scope and filters, plus --category Cost for a Cost type.")]
    public bool? Prioritized { get; set; }

    [Option(Description = "Filter recommendations by category. Allowed values are Cost, HighAvailability, Security, Performance, and OperationalExcellence. Matched case-insensitively against recommendation-type metadata. Cost with subcategory IdleResources also returns complete savings and pricing-availability aggregates in summary.cost, independent of --top and --prioritized.")]
    public string? Category { get; set; }

    [Option(Description = "Filter recommendations by business impact. Allowed values are High, Medium, and Low. Matched case-insensitively against recommendation-type metadata.")]
    public string? Impact { get; set; }

    [Option(Description = "Filter recommendations by lifecycle state. Allowed values are New, Postponed, Dismissed, and Completed. An explicit status always takes precedence. When omitted, historical inputs default to Completed; otherwise status grouping includes all states and other groupings default to New.")]
    public RecommendationStatus? Status { get; set; }

    [Option(Description = "Filter recommendations by a recommendation type ID GUID. Matching is case-insensitive.")]
    public string? RecommendationTypeId { get; set; }

    [Option(Description = "Filter recommendations by one or more recommendation type ID GUIDs using OR semantics. May be combined with --recommendation-type-id.")]
    public string[]? RecommendationTypeIds { get; set; }

    [Option(Description = "Filter recommendations by an impacted Azure resource type in properties.impactedField. Matching is case-insensitive.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Filter recommendations by one or more impacted Azure resource types using OR semantics. Each value uses the same matching rules as --resource-type and may be combined with it.")]
    public string[]? ResourceTypes { get; set; }

    [Option(Description = "Filter recommendations by Azure product family before aggregation and ranking, with or without prioritization. Supported values are Compute (including App Service plans), Containers, Databases, Networking, Storage, and Web. Metadata service family takes precedence, with resource-provider fallback when missing.")]
    public string? ProductFamily { get; set; }

    [Option(Description = "Filter recommendations by impacted resource name or full ARM resource ID. Uses a case-insensitive substring match.")]
    public string? Resource { get; set; }

    [Option(Description = "Filter recommendation problem text using a case-insensitive substring match. Prefer structured filters when an equivalent category, impact, subcategory, or resource-type filter exists.")]
    public string? Search { get; set; }

    [Option(Description = "Filter recommendations by metadata subcategory using a case-insensitive exact match. Known values include IdleResources, ServiceUpgradeAndRetirement, ZoneResiliency, and RegionalResiliency; future non-empty catalog values are accepted.")]
    public string? SubCategory { get; set; }

    [Option(Description = "Filter recommendations by service-retirement date in '<operator>:<yyyy-MM-dd>' format, for example 'le:2026-12-31'. Supported operators are eq, lt, le, gt, and ge. The ServiceUpgradeAndRetirement subcategory is inferred when omitted.")]
    public string? RetirementDate { get; set; }

    [Option(Description = "For Cost summaries, include only recommendations whose annualized retail potential savings (daily savings × 30.4 × 12) are at least this non-negative amount.")]
    public decimal? MinimumSavings { get; set; }

    [Option(Description = "For Cost summaries, calculate savings as of this UTC timestamp. Use an ISO 8601 timestamp such as 2026-09-30T23:59:59Z. Defaults status to Completed unless explicitly supplied. Historical values are Advisor estimates, not billing data.")]
    public DateTimeOffset? AsOf { get; set; }

    [Option(Description = "For historical Cost summaries, filter completed recommendations by deletion on or after this UTC timestamp. Defaults status to Completed unless explicitly supplied.")]
    public DateTimeOffset? From { get; set; }

    [Option(Description = "For historical Cost summaries, filter completed recommendations by deletion on or before this UTC timestamp. Defaults status to Completed unless explicitly supplied.")]
    public DateTimeOffset? To { get; set; }

    [Option(Description = "Optional display cap from 1 through 100 on the number of known buckets returned. Defaults to 10 with --prioritized true, where buckets keep their priority order. TotalRecommendations and summary.cost still reflect the complete filtered population (with --prioritized true, TotalRecommendations is the sum of affected resources per type), and in non-prioritized summaries an Unknown bucket is preserved after the selected buckets.")]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = "One or more subscription IDs or names to query. May be combined with --subscription; aliases of the same subscription are queried once. Specify either subscription scope or --service-group.")]
    public string[]? Subscriptions { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
