// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Helpers;

public static class CommandHelper
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

    public static string? GetSubscription(string? subscriptionValue)
    {
        subscriptionValue = subscriptionValue?.Trim('"', '\'');

        if (!string.IsNullOrEmpty(subscriptionValue) && !IsPlaceholder(subscriptionValue))
        {
            return subscriptionValue;
        }

        var defaultSubscription = GetDefaultSubscription();
        return !string.IsNullOrEmpty(defaultSubscription)
            ? defaultSubscription
            : subscriptionValue;
    }

    /// <summary>
    /// Gets the default subscription from the Azure CLI profile (~/.azure/azureProfile.json),
    /// falling back to the AZURE_SUBSCRIPTION_ID environment variable.
    /// The CLI profile read is cached for the lifetime of the process to avoid redundant file I/O.
    /// </summary>
    public static string? GetDefaultSubscription()
    {
        // Primary: Azure CLI profile (set via 'az account set') - cached to avoid repeated file I/O
        var profileDefault = GetProfileSubscription();
        if (!string.IsNullOrEmpty(profileDefault))
        {
            return profileDefault;
        }

        // Fallback: AZURE_SUBSCRIPTION_ID environment variable (cheap, not cached)
        return EnvironmentHelpers.GetAzureSubscriptionId();
    }

    internal static string? GetProfileSubscription() => s_profileDefault.Value;

    /// <summary>
    /// Checks if the given <paramref name="value"/> is a common placeholder for subscription ID or name.
    /// </summary>
    private static bool IsPlaceholder(string value) => s_subscriptionPlaceholders.Contains(value);
}
