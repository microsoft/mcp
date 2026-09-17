// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Advisor.Models;

namespace Azure.Mcp.Tools.Advisor.Services.Models;

internal sealed record RecommendationStatePatchProperties(
    [property: JsonPropertyName("recommendationStatus")] RecommendationStatus RecommendationStatus,
    [property: JsonPropertyName("postponedUntilDateTime")] DateTimeOffset? PostponedUntilDateTime = null,
    [property: JsonPropertyName("recommendationDismissReason")] RecommendationDismissReason? RecommendationDismissReason = null);
