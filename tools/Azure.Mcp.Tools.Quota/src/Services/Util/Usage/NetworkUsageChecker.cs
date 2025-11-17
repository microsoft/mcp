// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.Network;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class NetworkUsageChecker(TokenCredential credential, string subscriptionId, ILogger<NetworkUsageChecker> logger) : AzureUsageChecker(credential, subscriptionId, logger)
{
    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
            var usages = subscription.GetUsagesAsync(location, cancellationToken);
            var result = new List<UsageInfo>();

            await foreach (var item in usages)
            {
                result.Add(new UsageInfo(
                    Name: item.Name?.Value ?? string.Empty,
                    Limit: (int)item.Limit,
                    Used: (int)item.CurrentValue,
                    Unit: item.Unit.ToString()
                ));
            }

            return result;
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Failed to fetch Network quotas. Please check your subscription permissions and service availability.", error);
        }
    }
}
