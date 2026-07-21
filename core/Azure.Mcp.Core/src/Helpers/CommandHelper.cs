// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using Azure.Mcp.Core.Models.Option;

namespace Azure.Mcp.Core.Helpers
{
    public static class CommandHelper
    {
        /// <summary>
        /// Checks if a subscription is available either from the command option or AZURE_SUBSCRIPTION_ID environment variable.
        /// </summary>
        /// <param name="commandResult">The command result to check for the subscription option.</param>
        /// <returns>True if a subscription is available, false otherwise.</returns>
        public static bool HasSubscriptionAvailable(CommandResult commandResult)
        {
            var hasOption = commandResult.HasOptionResult(OptionDefinitions.Common.Subscription.Name);
            var hasEnv = !string.IsNullOrEmpty(EnvironmentHelpers.GetAzureSubscriptionId());
            return hasOption || hasEnv;
        }

        public static string? GetSubscription(ParseResult parseResult)
        {
            // Get subscription from command line option or fallback to environment variable
            var subscriptionValue = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription.Name);

            var envSubscription = EnvironmentHelpers.GetAzureSubscriptionId();
            return (string.IsNullOrEmpty(subscriptionValue) || IsPlaceholder(subscriptionValue)) && !string.IsNullOrEmpty(envSubscription)
                ? envSubscription
                : subscriptionValue;
        }

        private static bool IsPlaceholder(string value) => value.Contains("subscription") || value.Contains("default");
    }
}
