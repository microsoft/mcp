// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed class RecommendationProperties
{
    /// <summary> The category of the recommendation. </summary>
    public string? Category { get; set; }

    /// <summary> The business impact of the recommendation (e.g., High, Medium, Low). </summary>
    public string? Impact { get; set; }

    public string? Control { get; set; }

    public string? ImpactedField { get; set; }

    public string? ImpactedValue { get; set; }

    public string? RecommendationStatus { get; set; }

    public string? RecommendationDismissReason { get; set; }

    public DateTimeOffset? PostponedUntilDateTime { get; set; }

    public DateTimeOffset? LastRefreshed { get; set; }

    public DateTimeOffset? LastUpdated { get; set; }

    public DateTimeOffset? CreatedTime { get; set; }

    public string? RecommendationTypeId { get; set; }

    public string? CompletionType { get; set; }

    public string? Risk { get; set; }

    public string? Description { get; set; }

    public string? Label { get; set; }

    public string? LearnMoreLink { get; set; }

    public string? PotentialBenefits { get; set; }

    public string? SourceSystem { get; set; }

    public string? SuppressionId { get; set; }

    /// <summary> Short description of the recommendation. </summary>
    public RecommendationDescription? ShortDescription { get; set; }

    /// <summary> Metadata pertaining to the affected resource. </summary>
    public RecommendationResourceMetadata? ResourceMetadata { get; set; }
}
