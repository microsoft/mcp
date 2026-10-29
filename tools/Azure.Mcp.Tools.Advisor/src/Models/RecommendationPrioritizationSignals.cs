// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

/// <summary>
/// Type-level facts that explain why a recommendation type is ranked where it is in a prioritized summary.
/// </summary>
/// <remarks>
/// <para>
/// Every value appears once. Absent, unknown, or not-meaningful values are omitted rather than emitted as
/// <c>null</c>, <c>0</c>, or <c>Unknown</c>, so a caller can never mistake "no data" for "no savings" or "no risk".
/// </para>
/// <para>
/// <see cref="Category"/>, <see cref="Impact"/>, <see cref="SubCategory"/>, and <see cref="RetirementDate"/> come from the
/// recommendation-type metadata. The published <c>recommendationImpact</c> and <c>recommendationSubcategory</c> signals are
/// not emitted: the impact signal is a binary high/other bucket of <see cref="Impact"/>, and the subcategory signal is either
/// the same as <see cref="SubCategory"/> or a scoring bucket. The only extra information it carries, the retirement band,
/// is emitted as <see cref="RetirementWindow"/>.
/// </para>
/// <para>
/// Signal values are the published strings, unchanged, with their published casing; compare them case-insensitively.
/// See <c>PrioritizationSignalFilter</c> for which values are emitted.
/// </para>
/// </remarks>
public sealed record RecommendationPrioritizationSignals
{
    /// <summary>Metadata recommendation category, such as HighAvailability or Cost.</summary>
    [JsonPropertyName("category")]
    public string? Category { get; init; }

    /// <summary>Metadata business impact: High, Medium, or Low.</summary>
    [JsonPropertyName("impact")]
    public string? Impact { get; init; }

    /// <summary>Metadata subcategory, such as ZoneResiliency or ServiceUpgradeAndRetirement.</summary>
    [JsonPropertyName("subCategory")]
    public string? SubCategory { get; init; }

    /// <summary>Metadata service-retirement date in yyyy-MM-dd format.</summary>
    [JsonPropertyName("retirementDate")]
    public string? RetirementDate { get; init; }

    /// <summary>
    /// Published retirement band, for example retirementPastDue, retirementInDays25, or retirementWithin180Days.
    /// It is taken from the published signal and is not recomputed from <see cref="RetirementDate"/>.
    /// </summary>
    [JsonPropertyName("retirementWindow")]
    public string? RetirementWindow { get; init; }

    /// <summary>Sum of per-resource annual potential savings. Omitted when no resource reports savings.</summary>
    [JsonPropertyName("annualSavings")]
    public double? AnnualSavings { get; init; }

    /// <summary>Currency of <see cref="AnnualSavings"/>; emitted only with savings.</summary>
    [JsonPropertyName("annualSavingsCurrency")]
    public string? AnnualSavingsCurrency { get; init; }

    /// <summary>Published resource-type criticality, such as Networking, OperationalData, Compute, or Storage.</summary>
    [JsonPropertyName("resourceTypeCriticality")]
    public string? ResourceTypeCriticality { get; init; }

    /// <summary>Published critical-service signal; emitted only as qcs (a critical service).</summary>
    [JsonPropertyName("criticalService")]
    public string? CriticalService { get; init; }

    /// <summary>Published remediation effort: Low, Medium, or High.</summary>
    [JsonPropertyName("effort")]
    public string? Effort { get; init; }

    /// <summary>Published actionability; emitted only as OneClickOrFullyExecutable.</summary>
    [JsonPropertyName("actionability")]
    public string? Actionability { get; init; }

    /// <summary>Published cost implication, such as noCostImpact, lowCost, or otherCost (medium or high).</summary>
    [JsonPropertyName("costImplication")]
    public string? CostImplication { get; init; }
}
