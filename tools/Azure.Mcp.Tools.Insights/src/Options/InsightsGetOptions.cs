// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Insights.Options;

public class InsightsGetOptions : ISubscriptionOption
{
    [Option(InsightsOptionDefinitions.QueryDescription)]
    public string? Query { get; set; }

    [Option(InsightsOptionDefinitions.NoCacheDescription, Name = InsightsOptionDefinitions.NoCacheName)]
    public bool NoCache { get; set; }

    [Option(InsightsOptionDefinitions.ScopeDescription, DefaultValue = InsightsOptionDefinitions.ScopeSubscription)]
    public string? Scope { get; set; }

    [Option(OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(OptionDescriptions.AuthMethod, Name = "auth-method")]
    public AuthMethod? AuthMethod { get; set; }

    [Option(Name = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
