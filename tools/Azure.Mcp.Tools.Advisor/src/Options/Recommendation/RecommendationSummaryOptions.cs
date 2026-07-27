// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationSummaryOptions : RecommendationListOptions
{
    [Option(Description = "Optional field to group the summary by. One of: 'recommendation-type', 'category', 'impact', 'resource-type'. " +
        "Defaults to 'category' when omitted, which surfaces the high-level themes (Cost, Security, Reliability, etc.) " +
        "so prompts like 'summarize the key themes from my Advisor recommendations' work without naming a field.")]
    public string? GroupBy { get; set; }
}
