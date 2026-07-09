// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public class RecommendationListOptions : BaseAdvisorOptions
{
    public string? Category { get; set; }
    public string? Impact { get; set; }
    public string? ResourceType { get; set; }
    public string? Resource { get; set; }
    public string? Search { get; set; }
    public int? Top { get; set; }
}
