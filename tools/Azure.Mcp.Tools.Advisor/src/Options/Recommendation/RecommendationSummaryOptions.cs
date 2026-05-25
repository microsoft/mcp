// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public class RecommendationSummaryOptions : RecommendationListOptions
{
    public string? GroupBy { get; set; }
    public int? Top { get; set; }
}
