// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Optimization.Services;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Optimization.Options.Recommendation;

public class RecommendationListOptions : ISubscriptionOption
{
    [Option(Description = "Maximum number of cost-saving recommendations to return (1-1000, default 100). " +
        "Results are ranked by impact then currency-normalized annual savings.")]
    public int? Top { get; set; }

    [Option(Description = OptimizationStrings.SubscriptionOptionDescription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
