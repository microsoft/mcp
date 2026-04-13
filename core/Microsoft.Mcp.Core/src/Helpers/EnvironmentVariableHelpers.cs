// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Helpers;

public static class EnvironmentHelpers
{
    private const string AzureSubscriptionIdEnvironmentVariable = "AZURE_SUBSCRIPTION_ID";

    public static bool GetEnvironmentVariableAsBool(string envVarName)
        => Environment.GetEnvironmentVariable(envVarName) switch
        {
            "true" => true,
            "True" => true,
            "T" => true,
            "1" => true,
            _ => false
        };

    /// <summary>
    /// Gets the Azure subscription ID from the AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    /// <returns>The subscription ID if available, null otherwise.</returns>
    public static string? GetAzureSubscriptionId()
        => Environment.GetEnvironmentVariable(AzureSubscriptionIdEnvironmentVariable);

    /// <summary>
    /// Sets the AZURE_SUBSCRIPTION_ID environment variable. 
    /// This method is primarily intended for testing scenarios.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to set, or null to clear the variable.</param>
    /// <returns>Either the AZURE_SUBSCRIPTION_ID environment variable value that was set or the logged into Azure CLI subscription.</returns>
    public static string SetAzureSubscriptionId(string subscriptionId)
    {
        var currentSubscription = CommandHelper.GetProfileSubscription();
        if (!string.IsNullOrEmpty(currentSubscription))
        {
            return currentSubscription;
        }

        Environment.SetEnvironmentVariable(AzureSubscriptionIdEnvironmentVariable, subscriptionId);
        return subscriptionId;
    }

    /// <summary>
    /// Clears the AZURE_SUBSCRIPTION_ID environment variable.
    /// This is primarily intended for testing scenarios to ensure a clean environment state between tests.
    /// </summary>
    public static void ClearAzureSubscriptionId()
        => Environment.SetEnvironmentVariable(AzureSubscriptionIdEnvironmentVariable, null);

    public static bool IsPlaybackTesting()
    {
#if DEBUG
        // In debug builds, check for the presence of an environment variable to determine if we're in playback testing mode.
        var testModeEnv = Environment.GetEnvironmentVariable("TEST_MODE");
        return string.Equals(testModeEnv, "Playback", StringComparison.OrdinalIgnoreCase);
#else
        // In non-debug builds, never consider ourselves to be in playback testing mode.
        return false;
#endif
    }
}
