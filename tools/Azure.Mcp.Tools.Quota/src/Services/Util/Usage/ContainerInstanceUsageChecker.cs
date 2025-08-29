// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.ContainerInstance;
using Azure.ResourceManager.ContainerInstance.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class ContainerInstanceUsageChecker(TokenCredential credential, string subscriptionId, ILogger<ContainerInstanceUsageChecker> logger) : AzureUsageChecker(credential, subscriptionId, logger)
{
    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location)
    {
        try
        {
            var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
            var usages = subscription.GetUsagesWithLocationAsync(location);

            var result = new List<UsageInfo>();
            await foreach (ContainerInstanceUsage item in usages)
            {
                result.Add(new UsageInfo(
                    Name: item.Name?.LocalizedValue ?? item.Name?.Value ?? string.Empty,
                    Limit: item.Limit ?? 0,
                    Used: item.CurrentValue ?? 0,
                    Unit: item.Unit.ToString()
                ));
            }

            return result;
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Failed to fetch Container Instance quotas. Please check your subscription permissions and service availability.", error);
        }
    }
}
