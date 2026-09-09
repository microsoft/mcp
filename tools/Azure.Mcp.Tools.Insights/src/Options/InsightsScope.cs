// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Insights.Options;

public enum InsightsScope
{
    [JsonStringEnumMemberName(InsightsOptionDefinitions.ScopeSubscription)]
    Subscription,

    [JsonStringEnumMemberName(InsightsOptionDefinitions.ScopeTenant)]
    Tenant
}
