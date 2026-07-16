// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Insights.Services.Models;

// Aggregated infrastructure data for a scope (one subscription or all subscriptions in a tenant).
public sealed record SubscriptionAggregation(
    IReadOnlyDictionary<string, ResourceTypeAggregation> ResourceTypes,
    int SubscriptionCount,
    int ResourceGroupCount);
