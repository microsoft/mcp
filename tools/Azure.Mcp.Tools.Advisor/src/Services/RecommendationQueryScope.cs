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
    string? ResourceGroup,
    IReadOnlyList<string>? SubscriptionIds = null)
{
    internal string? SubscriptionId =>
        Kind == RecommendationQueryScopeKind.Subscription ? Value : null;

    internal IReadOnlyList<string> ScopedSubscriptionIds =>
        Kind == RecommendationQueryScopeKind.Subscription
            ? SubscriptionIds ?? [Value]
            : [];

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
            string.IsNullOrWhiteSpace(resourceGroup) ? null : resourceGroup.Trim(),
            [subscriptionId.Trim()]);
    }

    internal static RecommendationQueryScope ForSubscriptions(
        IReadOnlyCollection<string> subscriptionIds,
        string? resourceGroup)
    {
        ArgumentNullException.ThrowIfNull(subscriptionIds);
        var normalizedSubscriptionIds = subscriptionIds
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Select(static id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        ArgumentOutOfRangeException.ThrowIfZero(normalizedSubscriptionIds.Length);

        return new(
            RecommendationQueryScopeKind.Subscription,
            normalizedSubscriptionIds[0],
            string.IsNullOrWhiteSpace(resourceGroup) ? null : resourceGroup.Trim(),
            normalizedSubscriptionIds);
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
