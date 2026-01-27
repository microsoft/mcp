// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed class RecommendationProperties
{
    /// <summary> The category of the recommendation. </summary>
    public string? Category { get; set; }

    /// <summary> The creation date of the recommendation. </summary>
    [JsonPropertyName("lastUpdated")]
    public DateTimeOffset? CreatedOn { get; set; }

    /// <summary> Short description of the recommendation. </summary>
    public RecommendationDescription? ShortDescription { get; set; }

    /// <summary> Metadata pertaining to the affected resource. </summary>
    public RecommendationResourceMetadata? ResourceMetadata { get; set; }
}
