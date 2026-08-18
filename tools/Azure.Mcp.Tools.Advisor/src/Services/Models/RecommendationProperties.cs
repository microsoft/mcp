// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

/// <summary> Properties returned for an Advisor recommendation. </summary>
internal sealed class RecommendationProperties
{
    /// <summary> The recommendation category. </summary>
    public string? Category { get; set; }

    /// <summary> The Advisor control associated with the recommendation, such as Scalability. </summary>
    public string? Control { get; set; }

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

    /// <summary> The recommendation problem and solution text. </summary>
    public RecommendationDescription? ShortDescription { get; set; }

    /// <summary> Additional type-specific recommendation properties. </summary>
    public RecommendationExtendedProperties? ExtendedProperties { get; set; }

    /// <summary> Metadata pertaining to the affected resource. </summary>
    public RecommendationResourceMetadata? ResourceMetadata { get; set; }
}
