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
    IReadOnlyList<string>? SubscriptionIds = null,
    IReadOnlyList<string>? ServiceGroupIds = null)
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

    internal IReadOnlyList<string> ScopedServiceGroupIds =>
        Kind == RecommendationQueryScopeKind.ServiceGroup
            ? ServiceGroupIds ?? [Value]
            : [];

    internal IEnumerable<RecommendationQueryScope> Batch(int batchSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);
        var ids = Kind == RecommendationQueryScopeKind.Subscription
            ? ScopedSubscriptionIds
            : ScopedServiceGroupIds;
        for (var index = 0; index < ids.Count; index += batchSize)
        {
            var batch = ids.Skip(index).Take(batchSize).ToArray();
            yield return Kind == RecommendationQueryScopeKind.Subscription
                ? ForSubscriptions(batch, ResourceGroup)
                : ForServiceGroups(batch);
        }
    }

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
        => ForServiceGroups([serviceGroup]);

    internal static RecommendationQueryScope ForServiceGroups(
        IReadOnlyCollection<string> serviceGroups)
    {
        ArgumentNullException.ThrowIfNull(serviceGroups);
        var normalizedServiceGroups = serviceGroups
            .Where(static id => !string.IsNullOrWhiteSpace(id))
            .Select(static id =>
            {
                RecommendationScopeValidator.ThrowIfInvalidServiceGroupId(id);
                return id.Trim();
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        ArgumentOutOfRangeException.ThrowIfZero(normalizedServiceGroups.Length);
        return new(
            RecommendationQueryScopeKind.ServiceGroup,
            normalizedServiceGroups[0],
            null,
            null,
            normalizedServiceGroups);
    }
}
