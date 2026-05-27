// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Insights.Options;

public static class InsightsOptionDefinitions
{
    public const string QueryName = "query";
    public const string ScopeName = "scope";

    public const string ScopeSubscription = "subscription";
    public const string ScopeTenant = "tenant";

    public static readonly Option<string> Query = new($"--{QueryName}")
    {
        Description = "Optional free-form description of what the insights will be used for " +
                      "(e.g. 'To build an internal finance web app with a relational database backend'). " +
                      "When provided, insights are tailored toward this scenario; when omitted, generic " +
                      "patterns are returned.",
        Required = false,
    };

    public static readonly Option<string> Scope = new($"--{ScopeName}")
    {
        Description = $"Aggregation scope: '{ScopeSubscription}' (default) scans a single subscription " +
                      $"(uses --subscription when provided, otherwise the default subscription); " +
                      $"'{ScopeTenant}' scans every accessible subscription in the tenant.",
        Required = false,
        DefaultValueFactory = _ => ScopeSubscription,
    };
}
