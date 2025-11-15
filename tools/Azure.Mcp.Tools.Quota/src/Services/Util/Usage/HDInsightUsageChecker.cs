// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.ResourceManager.HDInsight;
using Azure.ResourceManager.HDInsight.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class HDInsightUsageChecker(TokenCredential credential, string subscriptionId, ILogger<HDInsightUsageChecker> logger) : AzureUsageChecker(credential, subscriptionId, logger)
{
    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = ResourceClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{SubscriptionId}"));
            var usages = subscription.GetHDInsightUsagesAsync(location, cancellationToken);
            var result = new List<UsageInfo>();

            await foreach (HDInsightUsage item in usages)
            {
                result.Add(new UsageInfo(
                     Name: item.Name?.LocalizedValue ?? item.Name?.Value ?? string.Empty,
                     Limit: (int)(item.Limit ?? 0),
                     Used: (int)(item.CurrentValue ?? 0),
                     Unit: item.Unit.ToString()
                 ));
            }
            return result;
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Failed to fetch HDInsight quotas. Please check your subscription permissions and service availability.", error);
        }
    }
}
