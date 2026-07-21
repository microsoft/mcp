// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Helpers
{
    public static class EnvironmentHelpers
    {
        private const string AzureSubscriptionIdEnvironmentVariable = "AZURE_SUBSCRIPTION_ID";

        public static bool GetEnvironmentVariableAsBool(string envVarName)
        {
            return Environment.GetEnvironmentVariable(envVarName) switch
            {
                "true" => true,
                "True" => true,
                "T" => true,
                "1" => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets the Azure subscription ID from the AZURE_SUBSCRIPTION_ID environment variable.
        /// </summary>
        /// <returns>The subscription ID if available, null otherwise.</returns>
        public static string? GetAzureSubscriptionId()
        {
            return Environment.GetEnvironmentVariable(AzureSubscriptionIdEnvironmentVariable);
        }

        /// <summary>
        /// Sets the AZURE_SUBSCRIPTION_ID environment variable. 
        /// This method is primarily intended for testing scenarios.
        /// </summary>
        /// <param name="subscriptionId">The subscription ID to set, or null to clear the variable.</param>
        public static void SetAzureSubscriptionId(string? subscriptionId)
        {
            Environment.SetEnvironmentVariable(AzureSubscriptionIdEnvironmentVariable, subscriptionId);
        }
    }
}
