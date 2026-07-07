// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;

namespace Microsoft.Mcp.Tests.Helpers;

/// <summary>
/// Execution mode for tests integrating with the test proxy.
/// </summary>
public enum TestMode
{
    Live,
    Record,
    Playback
}

public static class TestEnvironment
{
    public static bool IsRunningInCi =>
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase) ||
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TF_BUILD"));

    /// <summary>
    /// Sets the AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    /// <param name="subscriptionId">The subscription ID to set, or null to clear the variable.</param>
    /// <returns>Either the AZURE_SUBSCRIPTION_ID environment variable value that was set or the logged into Azure CLI subscription.</returns>
    public static void SetAzureSubscriptionId(string subscriptionId)
        => Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, subscriptionId);

    /// <summary>
    /// Clears the AZURE_SUBSCRIPTION_ID environment variable.
    /// </summary>
    public static void ClearAzureSubscriptionId()
        => Environment.SetEnvironmentVariable(EnvironmentHelpers.AzureSubscriptionIdEnvironmentVariable, null);

    /// <summary>
    /// Skips the test if a default subscription is configured via Azure CLI profile or AZURE_SUBSCRIPTION_ID.
    /// This is useful for tests that validate missing required --subscription parameters,
    /// which have fallback to default subscription sources in SubscriptionCommand.
    /// </summary>
    public static void SkipIfDefaultSubscriptionConfigured()
    {
        if (!string.IsNullOrEmpty(CommandHelper.GetDefaultSubscription()))
        {
            Xunit.Assert.Skip(
                "An Azure CLI default subscription is configured; cannot validate missing --subscription. " +
                "Run 'az logout' or 'az account clear' to clear the profile, then restart the test runner.");
        }
    }
}
