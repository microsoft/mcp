// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RecommendationStatus>))]
public enum RecommendationStatus
{
    New,
    Postponed,
    Dismissed,
    Completed,
}
