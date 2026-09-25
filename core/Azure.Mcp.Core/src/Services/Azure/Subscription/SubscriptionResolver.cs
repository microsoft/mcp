// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Core.Services.Azure.Subscription;

/// <summary>
/// Default implementation that resolves subscriptions from command-line arguments,
/// Azure CLI profile, and the AZURE_SUBSCRIPTION_ID environment variable.
/// </summary>
public sealed class SubscriptionResolver : ISubscriptionResolver
{
    // Cache the Azure CLI profile read to avoid redundant file I/O.
    // The profile is read at most once per process invocation.
    private static readonly Lazy<string?> s_profileDefault = new(AzureCliProfileHelper.GetDefaultSubscriptionId);

    /// <summary>
    /// A set of common placeholder values for subscription ID/name options.
    /// </summary>
    private static readonly HashSet<string> s_subscriptionPlaceholders = new(StringComparer.OrdinalIgnoreCase)
    {
        "<subscription>",
        "<subscription-id>",
        "<subscription-name>",
        "<subscription-id-or-name>",
        "<subscriptionId>",
        "<subscription_id>",
        "<subscription_name>",
        "<sub-id>",
        "<your-subscription-id>",
        "your-subscription-id",
        "SUBSCRIPTION_ID",
        "{subscription}",
        "{subscriptionId}",
        "{subscription-id}",
        "{subscription_id}",
        "{subscription-name}",
        "{subscription-name-or-id}",
        "YOUR SUBSCRIPTION",
        "YOUR SUBSCRIPTION ID",
        "default",
        "<default>",
        "{default}",
        "default-sub",
        "<default-sub>",
        "{default-sub}",
        "default_sub",
        "default_subscription",
    };

    public string? ResolveSubscription(string? subscription) => ResolveSubscriptionInternal(this, subscription);

    internal static string? ResolveSubscriptionInternal(ISubscriptionResolver resolver, string? subscription)
    {
        subscription = subscription?.Trim('"', '\'');

        if (!string.IsNullOrEmpty(subscription) && !IsPlaceholder(subscription))
        {
            return subscription;
        }

        var defaultSubscription = GetDefaultSubscriptionIdInternal(resolver);
        return !string.IsNullOrEmpty(defaultSubscription)
            ? defaultSubscription
            : subscription;
    }

    public string? GetDefaultSubscriptionId() => GetDefaultSubscriptionIdInternal(this);

    internal static string? GetDefaultSubscriptionIdInternal(ISubscriptionResolver resolver)
    {
        // Primary: Azure CLI profile (set via 'az account set') - cached to avoid repeated file I/O
        var profileDefault = resolver.GetProfileSubscriptionId();
        if (!string.IsNullOrEmpty(profileDefault))
        {
            return profileDefault;
        }

        // Fallback: AZURE_SUBSCRIPTION_ID environment variable (cheap, not cached)
        return resolver.GetEnvironmentSubscriptionId();
    }

    public string? GetProfileSubscriptionId() => s_profileDefault.Value;

    public string? GetEnvironmentSubscriptionId() => EnvironmentHelpers.GetAzureSubscriptionId();

    /// <summary>
    /// Checks if the given <paramref name="value"/> is a common placeholder for subscription ID or name.
    /// </summary>
    private static bool IsPlaceholder(string value) => s_subscriptionPlaceholders.Contains(value);
}
