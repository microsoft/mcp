// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

/// <summary> Options for filtering and limiting Advisor recommendation list results. </summary>
public class RecommendationListOptions : IRecommendationScopeOptions
{
    [Option(Description = "Filter recommendations by category (e.g., 'Security', 'Cost', 'Performance', 'HighAvailability', 'OperationalExcellence'). Case-insensitive exact match against English recommendation metadata.")]
    public string? Category { get; set; }

    [Option(Description = "Filter recommendations by business impact ('High', 'Medium', or 'Low'). Case-insensitive exact match against English recommendation metadata.")]
    public string? Impact { get; set; }

    [Option(Description = "Filter recommendations by status ('New', 'Postponed', 'Dismissed', or 'Completed'). Defaults to 'New' when omitted.")]
    public RecommendationStatus? Status { get; set; }

    [Option(Description = "Filter recommendations by the recommendation type ID GUID. " +
        "Uses a case-insensitive exact match and can be combined with other filters.")]
    public string? RecommendationTypeId { get; set; }

    [Option(Description = "Filter recommendations by impacted Azure resource type (e.g., 'Microsoft.Storage/storageAccounts'). " +
        "Requires a case-insensitive exact match against the supported resource type in English recommendation metadata " +
        "and a case-insensitive substring match in the instance resource ID.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Filter recommendations by a case-insensitive substring of the impacted resource's full ARM resource ID. " +
        "Accepts a resource name or resource ID.")]
    public string? Resource { get; set; }

    [Option(Description = "Free-text filter applied to the recommendation problem text (case-insensitive substring match). " +
        "Prefer a structured filter when it expresses the request; use search for topics such as 'encryption' or 'right-size'. " +
        "Matches the original instance problem text before metadata enrichment.")]
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

    [Option(Description = "Filter recommendations by the service-retirement date in English recommendation metadata, using '<operator>:<yyyy-MM-dd>' format, for example 'ge:2026-03-31'. " +
        "Supported operators are eq, lt, le, gt, and ge. Can be combined with --tracking-ids. --sub-category may be omitted; " +
        "when specified, it must be ServiceUpgradeAndRetirement.")]
    public string? RetirementDate { get; set; }

    [Option(Description = "Maximum number of recommendation records to return. Defaults to 50 and is clamped to the server-side range of 1 through 100.")]
    public int? Top { get; set; }

    [Option(Description = "List recommendations for an Azure Service Group instead of a subscription, " +
        "provided as the name segment of its ARM resource ID (the '{serviceGroupName}' in " +
        "'/providers/Microsoft.Management/serviceGroups/{serviceGroupName}'). " +
        "Specify either --service-group or --subscription, not both; --resource-group applies only to subscription scope. " +
        "Other filters (category, impact, status, search, etc.) and --prioritized still apply.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "Sort individual records by criticalityScore descending, treating missing scores as zero. " +
        "Only an explicit --category Cost selects savings ordering: savings.retail.dailyPotentialSavings descending, " +
        "falling back to extendedProperties.annualSavingsAmount. Missing, invalid, or non-finite savings sort last; zero is valid. " +
        "Ties sort by effective retirement date ascending (instance date with metadata fallback, missing dates last), " +
        "then resource name A-Z (case-insensitive). " +
        "Defaults to false; when false, no explicit ordering is applied. Applies to both subscription and Service Group scopes.")]
    public bool? Prioritized { get; set; }

    [Option(Description = "Include properties.signalBreakdown from Azure Resource Graph when present, preserving the original signal names and values. " +
        "Defaults to false. Setting this option to true requires --prioritized true; otherwise validation fails. " +
        "Including signals does not change filtering or ordering. Applies to both subscription and Service Group scopes. " +
        "The field is omitted when signals are unavailable.")]
    public bool? ShowPrioritizationSignals { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public string? ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
