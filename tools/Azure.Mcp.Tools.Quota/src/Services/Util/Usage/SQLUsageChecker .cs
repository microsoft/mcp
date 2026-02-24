// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class SQLUsageChecker(TokenCredential credential, string subscriptionId, ILogger<SQLUsageChecker> logger, IHttpClientFactory httpClientFactory, ITenantService tenantService) : AzureUsageChecker(credential, subscriptionId, logger, tenantService)
{
    private const string ServerQuotaMagicString = "ServerQuota";

    private static readonly string[] SkuNameList =
    [
        "ServerQuota",
        "RegionalVCoreQuotaForSQLDBAndDW",
        "SubscriptionFreeDatabaseCount",
        "SubnetQuota"
    ];

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location, CancellationToken cancellationToken)
    {
        try
        {
            var apiVersion = "2015-05-01-preview";
            var requestUrl = $"{GetManagementEndpoint()}/subscriptions/{SubscriptionId}/providers/Microsoft.Sql/locations/{location}/usages?api-version={apiVersion}";
            using var rawResponse = await GetQuotaByUrlAsync(requestUrl, cancellationToken);

            if (rawResponse?.RootElement.TryGetProperty("value", out var valueElement) != true)
            {
                return CreateEmptyQuotaInfo();
            }

            // Check if ServerQuota is available
            foreach (var item in valueElement.EnumerateArray())
            {
                if (item.TryGetProperty("name", out var nameElement) &&
                    nameElement.GetStringSafe() == ServerQuotaMagicString)
                {
                    var limit = 0;
                    var used = 0;

                    if (item.TryGetProperty("properties", out var propsElement))
                    {
                        if (propsElement.TryGetProperty("limit", out var limitElement))
                        {
                            limit = limitElement.GetInt32();
                        }

                        if (propsElement.TryGetProperty("currentValue", out var usedElement))
                        {
                            used = usedElement.GetInt32();
                        }
                    }

                    if (limit - used <= 0)
                    {
                        Logger.LogWarning("No ServerQuota available for SQL in location: {Location}", location);
                        return CreateEmptyQuotaInfo();
                    }

                    break;
                }
            }

            var result = new List<UsageInfo>();
            foreach (var item in valueElement.EnumerateArray())
            {
                var name = string.Empty;

                if (item.TryGetProperty("name", out var nameElement))
                {
                    name = nameElement.GetStringSafe();
                }

                // Filter by specific SKU names
                if (!SkuNameList.Contains(name))
                {
                    continue;
                }

                var limit = 0;
                var used = 0;
                var unit = string.Empty;

                if (item.TryGetProperty("properties", out var propsElement))
                {
                    if (propsElement.TryGetProperty("limit", out var limitElement))
                    {
                        limit = limitElement.GetInt32();
                    }

                    if (propsElement.TryGetProperty("currentValue", out var usedElement))
                    {
                        used = usedElement.GetInt32();
                    }

                    if (propsElement.TryGetProperty("unit", out var unitElement))
                    {
                        unit = unitElement.GetStringSafe();
                    }
                }

                // Format name with SKU details
                var displayName = GetSkuDetail(name, limit - used);
                result.Add(new UsageInfo(displayName, limit, used, unit));
            }

            return result;
        }
        catch (Exception error)
        {
            Logger.LogError(error, "Error fetching SQL quotas");
            throw new InvalidOperationException($"Failed to fetch SQL quotas. {error.Message}", error);
        }
    }

    private static string GetSkuDetail(string name, int remainingQuota)
    {
        if (string.Equals(name, "serverquota", StringComparison.OrdinalIgnoreCase))
        {
            return $"{name} (must be checked for all models, remaining quota: {remainingQuota})";
        }

        if (string.Equals(name, "regionalvcorequotaforsqldbanddw", StringComparison.OrdinalIgnoreCase))
        {
            return $"{name} (must be checked if choose vCore model, remaining quota: {remainingQuota})";
        }

        if (string.Equals(name, "subscriptionfreedatabasecount", StringComparison.OrdinalIgnoreCase))
        {
            return $"{name} (must be checked if choose free tier, remaining quota: {remainingQuota})";
        }

        return $"{name} (remaining quota: {remainingQuota})";
    }

    private static List<UsageInfo> CreateEmptyQuotaInfo() =>
    [
        new UsageInfo(Name: "No SKU available", Limit: 0, Used: 0)
    ];

    protected async Task<JsonDocument?> GetQuotaByUrlAsync(string requestUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await Credential.GetTokenAsync(new TokenRequestContext([$"{GetManagementEndpoint()}/.default"]), cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpClient = _httpClientFactory.CreateClient(nameof(SQLUsageChecker));
            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP error! status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonDocument.Parse(content);
        }
        catch (Exception error)
        {
            Logger.LogWarning("Error fetching quotas directly: {Error}", error.Message);
            return null;
        }
    }
}
