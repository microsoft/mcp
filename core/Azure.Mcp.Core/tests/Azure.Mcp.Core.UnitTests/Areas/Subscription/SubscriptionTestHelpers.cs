// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;

namespace Azure.Mcp.Core.UnitTests.Areas.Subscription;

/// <summary>
/// Helper methods for creating test subscription data using the Azure SDK model factories.
/// This follows the recommended pattern from:
/// https://learn.microsoft.com/en-us/dotnet/azure/sdk/unit-testing-mocking
/// </summary>
public static class SubscriptionTestHelpers
{
    public static SubscriptionData CreateSubscriptionData(string subscriptionId, string displayName)
    {
        // Convert string ID to valid subscription ResourceIdentifier
        var subGuid = Guid.NewGuid(); // Use random GUID for tests
        var subPath = $"/subscriptions/{subGuid}";
        var resourceId = new ResourceIdentifier(subPath);

        // Create subscription policies using model factory
        var policies = ResourceManagerModelFactory.SubscriptionPolicies(
            locationPlacementId: "Public_2014-09-01",
            quotaId: "PayAsYouGo_2014-09-01",
            spendingLimit: SpendingLimit.Off);

        // Create subscription data using the official model factory
        return ResourceManagerModelFactory.SubscriptionData(
            resourceId,
            subscriptionId,
            displayName,
            subGuid,
            SubscriptionState.Enabled,
            policies,
            authorizationSource: "RoleBased",
            managedByTenants: [],
            tags: new Dictionary<string, string>());
    }

    /// <summary>
    /// Creates a subscription with minimal test data - useful when you only need ID and name
    /// </summary>
    public static SubscriptionData CreateMinimalSubscriptionData(string id, string name) =>
        CreateSubscriptionData(id, name);

    /// <summary>
    /// Creates a list of test subscriptions with sequential IDs
    /// </summary>
    public static List<SubscriptionData> CreateTestSubscriptions(int count)
    {
        var subs = new List<SubscriptionData>();
        for (int i = 1; i <= count; i++)
        {
            subs.Add(CreateMinimalSubscriptionData(
                $"sub-{i}",
                $"Test Subscription {i}"));
        }
        return subs;
    }
}
