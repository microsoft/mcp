// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Metadata;

public sealed class RecommendationMetadataListOptions
{
    [Option(
        Description = "Language for localized recommendation metadata. Defaults to English ('en'). " +
            "Regional tags such as en-US fall back to their supported base language; catalog locales such as pt-BR, pt-PT, zh-Hans, and zh-Hant are preserved.",
        DefaultValue = "en")]
    public string Language { get; set; } = "en";

    [Option(Description = "Optional exact Azure resource type filter, such as 'microsoft.compute/virtualmachines'. Use it during brownfield onboarding to discover recommendation types applicable to that resource type. Matched case-insensitively.")]
    public string? ResourceType { get; set; }

    [Option(Description = "Optional recommendation impact filter. Allowed values are High, Medium, or Low. Matched case-insensitively; unfiltered results are ordered High, Medium, then Low.")]
    public string? Impact { get; set; }

    [Option(Description = "Optional exact Advisor category filter. Allowed values are Cost, HighAvailability, Security, Performance, and OperationalExcellence. Matched case-insensitively. ServiceUpgradeAndRetirement metadata and service-retirement filters require HighAvailability.")]
    public string? Category { get; set; }

    [Option(Description = "Optional exact recommendation subcategory filter, such as ServiceUpgradeAndRetirement, ZoneResiliency, or RegionalResiliency. Matched case-insensitively. ServiceUpgradeAndRetirement belongs to the HighAvailability category.")]
    public string? SubCategory { get; set; }

    [Option(Description = "Optional exact Service Health tracking ID filter, such as QNY1-HB8. Matched case-insensitively. Applies only to HighAvailability/ServiceUpgradeAndRetirement metadata; --category and --sub-category may be omitted but cannot specify conflicting values.")]
    public string? TrackingId { get; set; }

    [Option(Description = "Optional service-retirement date filter in '<operator>:<yyyy-MM-dd>' format, for example 'ge:2026-03-31'. Supported operators are eq, lt, le, gt, and ge. Applies only to HighAvailability/ServiceUpgradeAndRetirement metadata; --category and --sub-category may be omitted but cannot specify conflicting values.")]
    public string? RetirementDate { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
