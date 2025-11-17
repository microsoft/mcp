// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.MachineLearning;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class MachineLearningUsageChecker(TokenCredential credential, string subscriptionId, ILogger<MachineLearningUsageChecker> logger) : AzureUsageChecker(credential, subscriptionId, logger)
{
    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
            var usages = subscription.GetMachineLearningUsagesAsync(location, cancellationToken);
            var result = new List<UsageInfo>();

            await foreach (var item in usages)
            {
                result.Add(new UsageInfo(
                    Name: item.Name?.Value ?? string.Empty,
                    Limit: (int)(item.Limit ?? 0),
                    Used: (int)(item.CurrentValue ?? 0),
                    Unit: item.Unit.ToString()
                ));
            }

            return result;
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Failed to fetch Machine Learning quotas. Please check your subscription permissions and service availability.", error);
        }
    }
}
