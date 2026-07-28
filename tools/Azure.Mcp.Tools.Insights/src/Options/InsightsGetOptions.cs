// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Insights.Options;

public class InsightsGetOptions : ISubscriptionOption
{
    [Option(Description = InsightsOptionDefinitions.QueryDescription)]
    public string? Query { get; set; }

    [Option(Description = InsightsOptionDefinitions.NoCacheDescription, Name = InsightsOptionDefinitions.NoCacheName)]
    public bool NoCache { get; set; }

    [Option(Description = InsightsOptionDefinitions.ScopeDescription, DefaultValue = InsightsOptionDefinitions.ScopeSubscription)]
    public string? Scope { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
