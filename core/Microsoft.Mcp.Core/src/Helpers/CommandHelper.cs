// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Core.Helpers
{
    public static class CommandHelper
    {
        /// <summary>
        /// Checks if a subscription is available from the command option, Azure CLI profile, or AZURE_SUBSCRIPTION_ID environment variable.
        /// </summary>
        /// <param name="commandResult">The command result to check for the subscription option.</param>
        /// <returns>True if a subscription is available, false otherwise.</returns>
        public static bool HasSubscriptionAvailable(CommandResult commandResult)
        {
            var hasOption = commandResult.HasOptionResult(OptionDefinitions.Common.Subscription.Name);
            var hasDefault = !string.IsNullOrEmpty(GetDefaultSubscription());
            return hasOption || hasDefault;
        }

        public static string? GetSubscription(ParseResult parseResult)
        {
            // Get subscription from command line option or fallback to default subscription
            var subscriptionValue = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription.Name);

            var defaultSubscription = GetDefaultSubscription();
            return (string.IsNullOrEmpty(subscriptionValue) || IsPlaceholder(subscriptionValue)) && !string.IsNullOrEmpty(defaultSubscription)
                ? defaultSubscription
                : subscriptionValue;
        }

        /// <summary>
        /// Gets the default subscription from the Azure CLI profile (~/.azure/azureProfile.json),
        /// falling back to the AZURE_SUBSCRIPTION_ID environment variable.
        /// </summary>
        public static string? GetDefaultSubscription()
        {
            // Primary: Azure CLI profile (set via 'az account set')
            var profileDefault = AzureCliProfileHelper.GetDefaultSubscriptionId();
            if (!string.IsNullOrEmpty(profileDefault))
            {
                return profileDefault;
            }

            // Fallback: AZURE_SUBSCRIPTION_ID environment variable
            return EnvironmentHelpers.GetAzureSubscriptionId();
        }

        private static bool IsPlaceholder(string value) => value.Contains("subscription") || value.Contains("default");
    }
}
