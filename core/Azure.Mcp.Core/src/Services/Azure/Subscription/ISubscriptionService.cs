// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Core.Services.Azure.Subscription;

public interface ISubscriptionService
{
    Task<List<SubscriptionData>> GetSubscriptions(string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<SubscriptionResource> GetSubscription(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    bool IsSubscriptionId(string subscription, string? tenant = null);
    Task<string> GetSubscriptionIdByName(string subscriptionName, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);
    Task<string> GetSubscriptionNameById(string subscriptionId, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default subscription ID from the Azure CLI profile (~/.azure/azureProfile.json).
    /// Falls back to the AZURE_SUBSCRIPTION_ID environment variable if the profile is unavailable.
    /// </summary>
    /// <returns>The default subscription ID if found, null otherwise.</returns>
    string? GetDefaultSubscriptionId();
}
