// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.Search;
using Azure.ResourceManager.Search.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class SearchUsageChecker(TokenCredential credential, string subscriptionId, ILogger<SearchUsageChecker> logger) : AzureUsageChecker(credential, subscriptionId, logger)
{
    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
            var usages = subscription.GetUsagesBySubscriptionAsync(location, cancellationToken: cancellationToken);
            var result = new List<UsageInfo>();

            await foreach (QuotaUsageResult item in usages)
            {
                result.Add(new UsageInfo(
                    Name: item.Name?.Value ?? string.Empty,
                    Limit: item.Limit ?? 0,
                    Used: item.CurrentValue ?? 0,
                    Unit: item.Unit.ToString()
                ));
            }

            return result;
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Failed to fetch Search quotas. Please check your subscription permissions and service availability.", error);
        }
    }
}
