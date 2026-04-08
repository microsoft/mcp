// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Microsoft.Mcp.Core.Helpers;

public static class CommandHelper
{
    // Cache the Azure CLI profile read to avoid redundant file I/O.
    // The profile is read at most once per process invocation.
    private static readonly Lazy<string?> s_profileDefault = new(AzureCliProfileHelper.GetDefaultSubscriptionId);

    /// <summary>
    /// Checks if a subscription is available from the command option, Azure CLI profile, or AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    /// <param name="commandResult">The command result to check for the subscription option.</param>
    /// <returns>True if a subscription is available, false otherwise.</returns>
    public static bool HasSubscriptionAvailable(CommandResult commandResult)
    {
        if (commandResult.HasOptionResult(OptionDefinitions.Common.Subscription.Name))
        {
            return true;
        }

        return !string.IsNullOrEmpty(GetDefaultSubscription());
    }

    public static string? GetSubscription(ParseResult parseResult)
    {
        // Get subscription from command line option or fallback to default subscription
        var subscriptionValue = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription.Name);

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
        var profileDefault = s_profileDefault.Value;
        if (!string.IsNullOrEmpty(profileDefault))
        {
            return profileDefault;
        }

        // Fallback: AZURE_SUBSCRIPTION_ID environment variable (cheap, not cached)
        return EnvironmentHelpers.GetAzureSubscriptionId();
    }

    private static bool IsPlaceholder(string value) => value.Contains("subscription") || value.Contains("default");
}
