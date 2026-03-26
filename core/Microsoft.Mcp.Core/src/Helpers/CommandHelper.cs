// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Core.Helpers
{
    public static class CommandHelper
    {
        // Cache the Azure CLI profile read to avoid redundant file I/O.
        // The profile is read at most once per process invocation.
        private static Lazy<string?> s_profileDefault = new(AzureCliProfileHelper.GetDefaultSubscriptionId);

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
        /// Gets the default subscription from the AZURE_SUBSCRIPTION_ID environment variable,
        /// falling back to the Azure CLI profile (~/.azure/azureProfile.json).
        /// The CLI profile read is cached for the lifetime of the process to avoid redundant file I/O.
        /// </summary>
        public static string? GetDefaultSubscription()
        {
            // Primary: AZURE_SUBSCRIPTION_ID environment variable (cheap, not cached)
            var envSubscription = EnvironmentHelpers.GetAzureSubscriptionId();
            if (!string.IsNullOrEmpty(envSubscription))
            {
                return envSubscription;
            }

            // Fallback: Azure CLI profile (set via 'az account set') - cached to avoid repeated file I/O
            return s_profileDefault.Value;
        }

        /// <summary>
        /// Resets the cached Azure CLI profile subscription for testing purposes.
        /// </summary>
        internal static void ResetProfileCacheForTesting(Func<string?>? factory = null)
        {
            s_profileDefault = new Lazy<string?>(factory ?? AzureCliProfileHelper.GetDefaultSubscriptionId);
        }

        private static bool IsPlaceholder(string value) => value.Contains("subscription") || value.Contains("default");
    }
}
