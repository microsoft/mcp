// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tests.Helpers;

public static class TestSubscriptionData
{
    private static readonly ConstructorInfo? s_constructor = typeof(SubscriptionData).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();

    public static SubscriptionData Create(string subscriptionId, string displayName)
    {
        if (s_constructor == null)
        {
            throw new InvalidOperationException("Could not find constructor for SubscriptionData");
        }

        // Create subscription data using reflection
        var data = (SubscriptionData)s_constructor.Invoke([]);

        // Use reflection to set the read-only properties
        typeof(SubscriptionData)
            .GetProperty(nameof(SubscriptionData.SubscriptionId))!
            .SetValue(data, subscriptionId);

        typeof(SubscriptionData)
            .GetProperty(nameof(SubscriptionData.DisplayName))!
            .SetValue(data, displayName);

        return data;
    }
}
