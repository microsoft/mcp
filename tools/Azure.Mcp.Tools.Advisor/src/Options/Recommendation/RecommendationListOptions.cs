// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

/// <summary> Options for filtering and limiting Advisor recommendation list results. </summary>
public class RecommendationListOptions : ISubscriptionOption
{
    [Option(Description = "Filter recommendations by category (e.g., 'Security', 'Cost', 'Performance', 'HighAvailability', 'OperationalExcellence'). Case-insensitive exact match.")]
    public string? Category { get; set; }

    [Option(Description = "Filter recommendations by business impact ('High', 'Medium', or 'Low'). Case-insensitive exact match.")]
    public string? Impact { get; set; }

    [Option(Description = "Filter recommendations by impacted Azure resource type (e.g., 'Microsoft.Storage/storageAccounts'). Case-insensitive exact match.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Filter recommendations by impacted resource name or full ARM resource ID. Case-insensitive substring match.")]
    public string? Resource { get; set; }

    [Option(Description = "Free-text filter applied to the recommendation problem text (case-insensitive substring match). " +
        "Use this whenever the user's request includes a topical phrase such as 'related to Microsoft Foundry', " +
        "'about encryption', 'mentioning right-size', or 'for Key Vault'. " +
        "Extract the salient noun(s) from the phrase (e.g., 'Foundry', 'encrypt', 'right-size', 'Key Vault') and pass them here.")]
    public string? Search { get; set; }

    [Option(Description = "Filter recommendations by recommendation subcategory, matched case-insensitively against the Advisor recommendation metadata. " +
        "Known values include ComputeOptimization, DataPerformance, DataProtectionAndRecovery, EfficiencyOptimization, FailureMitigation, GovernanceAndCompliance, MonitoringAndAlerting, NetworkOptimization, Other, RegionalResiliency, Reservations, SafeAndSecureDeployment, SavingsPlan, Scalability, ServiceUpgradeAndRetirement, StorageOptimization, UsageOptimization, and ZoneResiliency. " +
        "Advisor can add values over time, so other subcategories are accepted.")]
    public string? SubCategory { get; set; }

    [Option(Description = "Filter recommendations by one or more Service Health tracking IDs, such as QNY1-HB8. " +
        "Pass several IDs as space-separated values after one option, for example --tracking-ids QNY1-HB8 9G0V-_G8; recommendations matching any of them are returned. " +
        "Matched case-insensitively within ServiceUpgradeAndRetirement metadata. Can be combined with --retirement-date. " +
        "--sub-category may be omitted; when specified, it must be ServiceUpgradeAndRetirement.")]
    public string[]? TrackingIds { get; set; }

    [Option(Description = "Filter recommendations by service-retirement date in '<operator>:<yyyy-MM-dd>' format, for example 'ge:2026-03-31'. " +
        "Supported operators are eq, lt, le, gt, and ge. Can be combined with --tracking-ids. --sub-category may be omitted; " +
        "when specified, it must be ServiceUpgradeAndRetirement.")]
    public string? RetirementDate { get; set; }

    [Option(Description = "Maximum number of items to return. " +
        "For 'list': defaults to 50, clamped to 1-100 (server-side limit). " +
        "For 'summary': optional display cap on the number of buckets returned (defaults to all). " +
        "TotalRecommendations always reflects the complete filtered population regardless of --top.")]
    public int? Top { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
