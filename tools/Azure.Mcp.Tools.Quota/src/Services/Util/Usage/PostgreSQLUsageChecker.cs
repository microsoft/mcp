// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class PostgreSQLUsageChecker(TokenCredential credential, string subscriptionId, ILogger<PostgreSQLUsageChecker> logger, IHttpClientFactory httpClientFactory, ITenantService tenantService) : AzureUsageChecker(credential, subscriptionId, logger, tenantService, httpClientFactory)
{
    private const string CoresMagicString = "cores";
    private const int MinimumCoresRequired = 2;

    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var requestUrl = $"{GetManagementEndpoint()}/subscriptions/{SubscriptionId}/providers/Microsoft.DBforPostgreSQL/locations/{location}/resourceType/flexibleServers/usages?api-version=2023-06-01-preview";
            using var rawResponse = await GetQuotaByUrlAsync(requestUrl, cancellationToken);

            if (rawResponse?.RootElement.TryGetProperty("value", out var valueElement) != true)
            {
                return CreateEmptyQuotaInfo();
            }

            foreach (var item in valueElement.EnumerateArray())
            {
                if (item.TryGetProperty("name", out var nameElement) &&
                    nameElement.TryGetProperty("value", out var nameValue) &&
                    nameValue.GetStringSafe() == CoresMagicString)
                {
                    var limit = item.TryGetProperty("limit", out var limitElement) ? limitElement.GetInt32() : 0;
                    var used = item.TryGetProperty("currentValue", out var usedElement) ? usedElement.GetInt32() : 0;

                    if (limit - used < MinimumCoresRequired)
                    {
                        Logger.LogWarning("Insufficient cores quota for PostgreSQL in location: {Location}", location);
                        return CreateEmptyQuotaInfo();
                    }

                    break;
                }
            }

            var result = new List<UsageInfo>();
            foreach (var item in valueElement.EnumerateArray())
            {
                var name = string.Empty;
                var limit = 0;
                var used = 0;
                var unit = string.Empty;

                if (item.TryGetProperty("name", out var nameElement) && nameElement.TryGetProperty("value", out var nameValue))
                {
                    name = nameValue.GetStringSafe();
                }

                if (item.TryGetProperty("limit", out var limitElement))
                {
                    limit = limitElement.GetInt32();
                }

                if (item.TryGetProperty("currentValue", out var usedElement))
                {
                    used = usedElement.GetInt32();
                }

                if (item.TryGetProperty("unit", out var unitElement))
                {
                    unit = unitElement.GetStringSafe();
                }

                // Format name with SKU details
                var displayName = GetSkuDetail(name, limit - used);
                result.Add(new UsageInfo(displayName, limit, used, unit));
            }

            return result;
        }
        catch (Exception error)
        {
            Logger.LogError(error, "Error fetching PostgreSQL quotas");
            return [];
        }
    }

    // Microsoft.DBforPostgreSQL/flexibleServers: cores, standardBSFamily, standardDADSv5Family, standardDDSv4Family, standardDDSv5Family, standardDSv3Family, standardEADSv5Family, standardEDSv4Family, standardEDSv5Family, standardESv3Family
    private static string GetSkuDetail(string name, int remainingQuota)
    {
        if (name.StartsWith("standardB", StringComparison.OrdinalIgnoreCase))
        {
            return $"{name} (Burstable tier)";
        }

        if (name.StartsWith("standardD", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var skuParts = name[9..].Split("Family")[0];
                var prefix = skuParts[..^2].ToLowerInvariant();
                var suffix = skuParts[^2..];
                return $"{name} (GeneralPurpose tier, e.g. Standard_D2{prefix}_{suffix})";
            }
            catch (Exception)
            {
                return name;
            }
        }

        if (name.StartsWith("standardE", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var skuParts = name[9..].Split("Family")[0];
                var prefix = skuParts[..^2].ToLowerInvariant();
                var suffix = skuParts[^2..];
                return $"{name} (MemoryOptimized tier, e.g. Standard_E2{prefix}_{suffix})";
            }
            catch (Exception)
            {
                return name;
            }
        }

        if (string.Equals(name, CoresMagicString, StringComparison.OrdinalIgnoreCase))
        {
            return $"{name} (remaining quota: {remainingQuota})";
        }

        return name;
    }

    private static List<UsageInfo> CreateEmptyQuotaInfo() =>
    [
        new UsageInfo(Name: "No SKU available", Limit: 0, Used: 0)
    ];
}
