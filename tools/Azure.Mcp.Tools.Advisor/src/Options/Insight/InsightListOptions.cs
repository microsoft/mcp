// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Options.Insight;

/// <summary> Options for listing Advisor insights for a Service Group or subscription. </summary>
public sealed class InsightListOptions : ISubscriptionOption
{
    [Option(Description = "Service Group name (for example 'commerce'), not its full ARM ID. " +
        "Returns insights for that Service Group. Mutually exclusive with --subscription. " +
        "Omit both --service-group and --subscription to return insights across all accessible Service Groups and subscriptions.")]
    public string? ServiceGroup { get; set; }

    [Option(Description = "Filter by insight name (for example 'IdleResources', 'ZonalResiliency', 'ServiceRetirement'). " +
        "Case-insensitive exact match. Omit to return all insights for the scope.")]
    public string? InsightName { get; set; }

    [Option(Description = "Maximum number of insights to return (1-500, default 50).")]
    public int? Top { get; set; }

    [Option(Description = "Azure subscription ID or name. Returns insights for that subscription (only service retirement applies). " +
        "Mutually exclusive with --service-group. No default subscription is applied.")]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
