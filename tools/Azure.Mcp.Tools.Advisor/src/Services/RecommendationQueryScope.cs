// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Advisor.Validation;

namespace Azure.Mcp.Tools.Advisor.Services;

internal enum RecommendationQueryScopeKind
{
    Subscription,
    ServiceGroup,
}

internal sealed record RecommendationQueryScope(
    RecommendationQueryScopeKind Kind,
    string Value,
    string? ResourceGroup)
{
    internal string? SubscriptionId =>
        Kind == RecommendationQueryScopeKind.Subscription ? Value : null;

    internal string? ServiceGroup =>
        Kind == RecommendationQueryScopeKind.ServiceGroup ? Value : null;

    internal string? ServiceGroupResourceId =>
        ServiceGroup is null
            ? null
            : RecommendationQueryBuilder.ServiceGroupResourceIdPrefix + ServiceGroup;

    internal static RecommendationQueryScope ForSubscription(
        string subscriptionId,
        string? resourceGroup)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
        return new(
            RecommendationQueryScopeKind.Subscription,
            subscriptionId.Trim(),
            string.IsNullOrWhiteSpace(resourceGroup) ? null : resourceGroup.Trim());
    }

    internal static RecommendationQueryScope ForServiceGroup(string serviceGroup)
    {
        RecommendationScopeValidator.ThrowIfInvalidServiceGroupId(serviceGroup);
        return new(
            RecommendationQueryScopeKind.ServiceGroup,
            serviceGroup.Trim(),
            null);
    }
}
