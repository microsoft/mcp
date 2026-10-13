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

    [Option(Description = "Rank active recommendation types by priority instead of counting them. " +
        "Set to true for top, most critical, most important, highest priority, prioritized, or what-should-I-fix-first requests, in all categories or a filtered category such as Cost; pass a user-specified result count as --top. " +
        "Do not set it for frequency questions such as how many, most common, or which type occurs most. " +
        "Types are ranked by metadata priority score, then annual savings, summed contextual criticality score, earliest service-retirement date, and affected-resource count. " +
        "Groups by recommendation-type, includes only New recommendations, and defaults --top to 10. Defaults to false. " +
        "Each group's count is its number of distinct affected resources. Each group's prioritizationSignals object can contain category, impact, subCategory, retirementDate, retirementWindow, annualSavings with annualSavingsCurrency, " +
        "and published signals such as resourceTypeCriticality, criticalService, effort, actionability, and costImplication; an absent field means not available, never zero. " +
        "To explain why a type is prioritized, answer from its prioritizationSignals without another call in two or three plain business sentences that lead with the most important present attribute, in the order " +
        "impact, retirementWindow or subCategory, resourceTypeCriticality, criticalService, effort, actionability, and costImplication. " +
        "Translate values into plain language, for example retirementPastDue means the retirement date has passed and qcs means a critical service; match values case-insensitively; " +
        "never mention field names, raw values, scores, or rank comparisons; never guess or describe absent attributes. " +
        "When the user then asks for details, instances, or affected resources of a listed type, identified by key, name, or description, call recommendation list with --recommendation-type-id set to that group's key " +
        "and --prioritized true, preserving the scope and filters, and add --category Cost for a Cost type. " +
        "Use metadata get to explain what a recommendation type means and remediation get to explain how to fix it.")]
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

    [Option(Description = "Optional display cap from 1 through 100 on the number of known buckets returned. Defaults to 10 with --prioritized true, where buckets keep their priority order. TotalRecommendations still reflects the complete filtered population, and in non-prioritized summaries an Unknown bucket is preserved after the selected buckets.")]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
