// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

/// <summary>Options for aggregating Advisor recommendations into per-bucket counts.</summary>
public sealed class RecommendationSummaryOptions : IRecommendationScopeOptions
{
    [Option(Description = "The ID of the Azure service group whose recommendations should be summarized, provided as the name segment in its ARM resource ID. Specify either --service-group or --subscription. If both are omitted, the configured default subscription is used.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "Optional field to group the summary by. One of: 'recommendation-type', 'category', 'impact', 'resource-type', 'status', 'sub-category', or 'retirement-date'. " +
        "Defaults to 'category' when omitted, which surfaces the high-level themes (Cost, Security, Reliability, etc.) " +
        "so prompts like 'summarize the key themes from my Advisor recommendations' work without naming a field. " +
        "Use 'recommendation-type' without --prioritized for frequency questions such as the most common recommendation types; its counts are recommendation records. " +
        "With --prioritized true, omit this option; prioritized summaries are always grouped by recommendation type.")]
    public string? GroupBy { get; set; }

    [Option(Description = "Rank active (New) recommendation types by priority instead of counting them. Defaults to false. " +
        "Set to true for top recommendations, top actions to take, most critical, most important, highest-priority, or what-should-I-fix-first requests, overall or within filters such as category Cost; " +
        "pass a requested count as --top (default 10). Do not set it for frequency questions such as how many or most common. Groups by recommendation-type. " +
        "Ranking: metadata priority score, then annual savings, summed criticality score, earliest retirement date, and affected-resource count. " +
        "Each group's count is its distinct affected resources, and totalRecommendations is the sum of those counts across all ranked types, not a record count; each group's prioritizationSignals object can hold category, impact, subCategory, retirementDate, retirementWindow, annualSavings (USD), " +
        "resourceTypeCriticality, criticalService, effort, actionability, and costImplication; absent fields are unknown, never zero. " +
        "To explain a ranking, answer from prioritizationSignals in two or three plain business sentences, leading with impact, then retirementWindow or subCategory, then the other signals; " +
        "translate values (retirementPastDue: retirement date has passed; qcs: critical service) and never mention field names, raw values, or scores, or guess absent attributes. " +
        "For details or affected resources of a listed type, call recommendation list with --recommendation-type-id set to its key and --prioritized true, keeping the scope and filters, plus --category Cost for a Cost type.")]
    public bool? Prioritized { get; set; }

    [Option(Description = "Filter recommendations by category. Allowed values are Cost, HighAvailability, Security, Performance, and OperationalExcellence. Matched case-insensitively against recommendation-type metadata.")]
    public string? Category { get; set; }

    [Option(Description = "Filter recommendations by business impact. Allowed values are High, Medium, and Low. Matched case-insensitively against recommendation-type metadata.")]
    public string? Impact { get; set; }

    [Option(Description = "Filter recommendations by one recommendation type ID GUID. Uses a case-insensitive exact match.")]
    public string? RecommendationTypeId { get; set; }

    [Option(Description = "Filter recommendations by the impacted Azure resource type in properties.impactedField, such as 'Microsoft.Storage/storageAccounts'. Uses a case-insensitive exact match.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Filter recommendations by impacted resource name or full ARM resource ID. Uses a case-insensitive substring match.")]
    public string? Resource { get; set; }

    [Option(Description = "Filter recommendation problem text using a case-insensitive substring match. Prefer structured filters when an equivalent category, impact, subcategory, or resource-type filter exists.")]
    public string? Search { get; set; }

    [Option(Description = "Filter recommendations by metadata subcategory using a case-insensitive exact match. Known values include ServiceUpgradeAndRetirement, ZoneResiliency, and RegionalResiliency; future non-empty catalog values are accepted.")]
    public string? SubCategory { get; set; }

    [Option(Description = "Filter recommendations by service-retirement date in '<operator>:<yyyy-MM-dd>' format, for example 'le:2026-12-31'. Supported operators are eq, lt, le, gt, and ge. The ServiceUpgradeAndRetirement subcategory is inferred when omitted.")]
    public string? RetirementDate { get; set; }

    [Option(Description = "Optional display cap from 1 through 100 on the number of known buckets returned. Defaults to 10 with --prioritized true, where buckets keep their priority order. TotalRecommendations still reflects the complete filtered population (with --prioritized true, the sum of affected resources per type), and in non-prioritized summaries an Unknown bucket is preserved after the selected buckets.")]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
