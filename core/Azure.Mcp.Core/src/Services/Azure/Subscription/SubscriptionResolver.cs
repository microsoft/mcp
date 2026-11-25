// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Services.Host;

namespace Azure.Mcp.Core.Services.Azure.Subscription;

/// <summary>
/// Default implementation that resolves subscriptions from command-line arguments,
/// Azure CLI profile, and the AZURE_SUBSCRIPTION_ID environment variable.
/// </summary>
public sealed class SubscriptionResolver(IFileSystem fileSystem, IEnvironmentVariables environmentVariables) : ISubscriptionResolver
{
    // Cache the Azure CLI profile read to avoid redundant file I/O.
    // Keep the cache local to this host's dependency-injection container.
    private readonly Lazy<string?> _profileDefault = new(() => AzureCliProfileHelper.GetDefaultSubscriptionId(fileSystem, environmentVariables));

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

    public string? ResolveSubscription(string? subscription)
    {
        var resolvedSubscription = ResolveSubscriptionInternal(this, subscription);
        if (!fileSystem.IsAccessAllowed && (string.IsNullOrWhiteSpace(resolvedSubscription) || IsPlaceholder(resolvedSubscription)))
        {
            throw new CommandValidationException("A subscription must be provided explicitly when host subscription defaults are unavailable.");
        }

        return resolvedSubscription;
    }

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

    public string? GetProfileSubscriptionId()
    {
        return fileSystem.IsAccessAllowed ? _profileDefault.Value : null;
    }

    public string? GetEnvironmentSubscriptionId() => environmentVariables.IsAccessAllowed
        ? environmentVariables.GetEnvironmentVariable("AZURE_SUBSCRIPTION_ID")
        : null;

    /// <summary>
    /// Checks if the given <paramref name="value"/> is a common placeholder for subscription ID or name.
    /// </summary>
    private static bool IsPlaceholder(string value) => s_subscriptionPlaceholders.Contains(value);
}
