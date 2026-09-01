// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Recommendation;

public sealed class RecommendationSummaryOptions : RecommendationListOptions
{
    [Option(Description = "Optional field to group the summary by.", DefaultValue = RecommendationGroupBy.Category)]
    public RecommendationGroupBy? GroupBy { get; set; }
}
