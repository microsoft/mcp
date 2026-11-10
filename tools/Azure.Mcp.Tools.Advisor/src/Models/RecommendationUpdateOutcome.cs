// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Advisor.Models;

[JsonConverter(typeof(JsonStringEnumConverter<RecommendationUpdateOutcome>))]
public enum RecommendationUpdateOutcome
{
    Succeeded,
    Failed,
    Unverified,
    NotAttempted,
}
