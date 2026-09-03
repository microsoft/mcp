// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

public enum AdvisorRecommendationGroupBy
{
    [JsonStringEnumMemberName("recommendation-type")]
    RecommendationType,

    [JsonStringEnumMemberName("category")]
    Category,

    [JsonStringEnumMemberName("impact")]
    Impact,

    [JsonStringEnumMemberName("resource-type")]
    ResourceType
}
