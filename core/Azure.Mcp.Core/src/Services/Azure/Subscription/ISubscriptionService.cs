// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.Subscription;

public interface ISubscriptionService
{

    Task<List<SubscriptionData>> GetSubscriptions(string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default, Uri? authorityHost = default);
    Task<SubscriptionResource> GetSubscription(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default, Uri? authorityHost = default);
    bool IsSubscriptionId(string subscription, string? tenant = null);
    Task<string> GetSubscriptionIdByName(string subscriptionName, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default, Uri? authorityHost = default);
    Task<string> GetSubscriptionNameById(string subscriptionId, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default, Uri? authorityHost = default);
}
