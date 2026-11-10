// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Models;

internal sealed record ResourceInsight(
    string RecommendationTypeId,
    string DisplayName,
    string ResourceType,
    IReadOnlyList<ResourceInsightCheck> Checks,
    IReadOnlyList<string> ManualConfirmations);
