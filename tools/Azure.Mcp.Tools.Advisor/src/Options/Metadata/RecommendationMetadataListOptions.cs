// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Metadata;

public sealed class RecommendationMetadataListOptions
{
    [Option(
        Description = "Language for localized recommendation metadata. Defaults to English ('en'). " +
            "Supported catalog languages are en, cs, de, es, fr, hu, id, it, ja, ko, nl, pl, pt-BR, pt-PT, ru, sv, tr, zh-Hans, and zh-Hant. " +
            "Regional variants map to a supported base language when available, such as en-US to en and fr-CA to fr; locale-specific catalog languages remain distinct.",
        DefaultValue = "en")]
    public string Language { get; set; } = "en";

    [Option(Description = "Optional exact Azure resource type filter, such as 'microsoft.compute/virtualmachines'. Use it during brownfield onboarding to discover recommendation types applicable to that resource type. Matched case-insensitively.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Optional recommendation impact filter. Unfiltered results are ordered by impact and then display name.")]
    public AdvisorImpact? Impact { get; set; }

    [Option(Description = "Optional exact Advisor category filter.")]
    public AdvisorCategory? Category { get; set; }

    [Option(Description = "Optional exact recommendation subcategory filter, matched case-insensitively. " +
        "Known catalog values include ComputeOptimization, DataPerformance, DataProtectionAndRecovery, EfficiencyOptimization, FailureMitigation, GovernanceAndCompliance, MonitoringAndAlerting, NetworkOptimization, Other, Personalized, RegionalResiliency, Reservations, SafeAndSecureDeployment, SavingsPlan, Scalability, ServiceUpgradeAndRetirement, StorageOptimization, UsageOptimization, and ZoneResiliency. " +
        "The catalog can add values over time, so other subcategories are accepted.")]
    public string? SubCategory { get; set; }

    [Option(Description = "Optional exact Service Health tracking ID filter, such as QNY1-HB8. Matched case-insensitively within ServiceUpgradeAndRetirement metadata. --sub-category may be omitted but cannot specify a different value.")]
    public string? TrackingId { get; set; }

    [Option(Description = "Optional service-retirement date filter in '<operator>:<yyyy-MM-dd>' format, for example 'ge:2026-03-31'. Supported operators are eq, lt, le, gt, and ge. Applies only to the ServiceUpgradeAndRetirement subcategory; --sub-category may be omitted but cannot specify a different value.")]
    public string? RetirementDate { get; set; }

}
