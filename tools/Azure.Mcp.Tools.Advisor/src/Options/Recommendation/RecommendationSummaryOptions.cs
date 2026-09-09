// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

/// <summary>Options for aggregating Advisor recommendations into per-bucket counts.</summary>
public sealed class RecommendationSummaryOptions : ISubscriptionOption
{
    [Option(Description = "Optional field to group the summary by. One of: 'recommendation-type', 'category', 'impact', 'resource-type', 'status', 'sub-category', or 'retirement-date'. " +
        "Defaults to 'category' when omitted, which surfaces the high-level themes (Cost, Security, Reliability, etc.) " +
        "so prompts like 'summarize the key themes from my Advisor recommendations' work without naming a field.")]
    public string? GroupBy { get; set; }

    [Option(Description = "Filter recommendations by category. Allowed values are Cost, HighAvailability, Security, Performance, and OperationalExcellence. Matched case-insensitively against metadata with recommendation-instance fallback.")]
    public string? Category { get; set; }

    [Option(Description = "Filter recommendations by business impact. Allowed values are High, Medium, and Low. Matched case-insensitively against metadata with recommendation-instance fallback.")]
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

    [Option(Description = "Optional display cap from 1 through 100 on the number of known buckets returned. TotalRecommendations still reflects the complete filtered population, and an Unknown bucket is preserved after the selected buckets.")]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
