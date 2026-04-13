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
        var subscriptionValue = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription.Name);
        return ResolveSubscription(subscriptionValue, GetDefaultSubscription());
    }

    /// <summary>
    /// Gets the default subscription from the Azure CLI profile (~/.azure/azureProfile.json),
    /// falling back to the AZURE_SUBSCRIPTION_ID environment variable.
    /// The CLI profile read is cached for the lifetime of the process to avoid redundant file I/O.
    /// </summary>
    public static string? GetDefaultSubscription()
        => ResolveDefaultSubscription(GetProfileSubscription(), EnvironmentHelpers.GetAzureSubscriptionId());

    internal static string? GetProfileSubscription() => s_profileDefault.Value;

    /// <summary>
    /// Pure resolution logic: returns the first non-empty subscription source.
    /// Priority: Azure CLI profile > AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    internal static string? ResolveDefaultSubscription(string? profileSubscription, string? envSubscription)
        => !string.IsNullOrEmpty(profileSubscription) ? profileSubscription : envSubscription;

    /// <summary>
    /// Pure resolution logic: returns the explicit option value if valid, otherwise the default.
    /// Placeholder values (containing "subscription" or "default") are treated as absent.
    /// </summary>
    internal static string? ResolveSubscription(string? optionValue, string? defaultSubscription)
    {
        if (!string.IsNullOrEmpty(optionValue) && !IsPlaceholder(optionValue))
        {
            return optionValue;
        }

        return !string.IsNullOrEmpty(defaultSubscription)
            ? defaultSubscription
            : optionValue;
    }

    private static bool IsPlaceholder(string value) => value.Contains("subscription") || value.Contains("default");
}
