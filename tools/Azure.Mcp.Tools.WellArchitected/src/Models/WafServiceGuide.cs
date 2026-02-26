// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.WellArchitected.Models;

public record WafServiceGuide(
    string Service,
    string Content)
{
    public List<string>? RecommendationIds { get; init; }

    public List<WafRecommendation> Recommendations { get; set; } = [];
};
