// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Insights.Options;

public static class InsightsOptionDefinitions
{
    public const string QueryName = "query";
    public const string NoCacheName = "nocache";
    public const string ScopeName = "scope";
    public const string SubscriptionName = "subscription";

    public const string ScopeSubscription = "subscription";
    public const string ScopeTenant = "tenant";

    public const string QueryDescription =
        "Optional free-form description of what the insights will be used for " +
        "(e.g. 'To build an internal finance web app with a relational database backend'). " +
        "When provided, insights are tailored toward this scenario; when omitted, generic " +
        "patterns are returned.";

    public const string NoCacheDescription =
        "Bypass the cached aggregation and force a fresh Azure Resource Graph scan. " +
        "The newly computed aggregation replaces the cached entry for the same scope.";

    public const string ScopeDescription =
        "Aggregation scope: '" + ScopeSubscription + "' (default) scans a single subscription " +
        "(uses --subscription when provided, otherwise the default subscription); " +
        "'" + ScopeTenant + "' scans every accessible subscription in the tenant.";
}
