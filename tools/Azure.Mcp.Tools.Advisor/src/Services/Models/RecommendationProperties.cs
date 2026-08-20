// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed class RecommendationProperties
{
    /// <summary> The recommendation category. </summary>
    public string? Category { get; set; }

    /// <summary> The recommendation type ID used to join with recommendation metadata. </summary>
    public string? RecommendationTypeId { get; set; }

    /// <summary> The recommendation impact. </summary>
    public string? Impact { get; set; }

    /// <summary> The resource type Advisor evaluated. </summary>
    public string? ImpactedField { get; set; }

    /// <summary> The resource name or value Advisor evaluated. </summary>
    public string? ImpactedValue { get; set; }

    /// <summary> The lifecycle state of the recommendation (e.g., New, Dismissed, Postponed). </summary>
    public string? RecommendationStatus { get; set; }

    /// <summary> The time the recommendation was first generated. </summary>
    public DateTimeOffset? CreatedTime { get; set; }

    /// <summary> The time the recommendation was last updated. </summary>
    [JsonPropertyName("lastUpdated")]
    public DateTimeOffset? LastUpdated { get; set; }

    /// <summary> The time Advisor last refreshed the recommendation evaluation. </summary>
    public DateTimeOffset? LastRefreshed { get; set; }

    /// <summary> Short description of the recommendation. </summary>
    public RecommendationDescription? ShortDescription { get; set; }

    public Dictionary<string, JsonElement>? Metadata { get; set; }

    /// <summary> Additional type-specific recommendation properties. </summary>
    public Dictionary<string, JsonElement>? ExtendedProperties { get; set; }

    /// <summary> Metadata pertaining to the affected resource. </summary>
    public RecommendationResourceMetadata? ResourceMetadata { get; set; }

    public string? Risk { get; set; }
    public string? Description { get; set; }
    public string? Label { get; set; }
    public string? LearnMoreLink { get; set; }
    public string? PotentialBenefits { get; set; }
    public JsonElement? Actions { get; set; }
    public JsonElement? Remediation { get; set; }
    public Dictionary<string, JsonElement>? ExposedMetadataProperties { get; set; }
    public JsonElement? TrackedProperties { get; set; }
    public JsonElement? Review { get; set; }
    public JsonElement? ResourceWorkload { get; set; }
    public string? SourceSystem { get; set; }
    public string? Notes { get; set; }
}
