// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class PostgreSQLUsageChecker(TokenCredential credential, string subscriptionId, ILogger<PostgreSQLUsageChecker> logger, IHttpClientFactory httpClientFactory, ITenantService tenantService) : AzureUsageChecker(credential, subscriptionId, logger, tenantService)
{
    private const string CoresMagicString = "cores";
    private const int MinimumCoresRequired = 2;

    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));

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
            var skuParts = name[9..].Split("Family")[0];
            var prefix = skuParts[..^2].ToLowerInvariant();
            var suffix = skuParts[^2..];
            return $"{name} (GeneralPurpose tier, e.g. Standard_D2{prefix}_{suffix})";
        }

        if (name.StartsWith("standardE", StringComparison.OrdinalIgnoreCase))
        {
            var skuParts = name[9..].Split("Family")[0];
            var prefix = skuParts[..^2].ToLowerInvariant();
            var suffix = skuParts[^2..];
            return $"{name} (MemoryOptimized tier, e.g. Standard_E2{prefix}_{suffix})";
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

    protected async Task<JsonDocument?> GetQuotaByUrlAsync(string requestUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await Credential.GetTokenAsync(new TokenRequestContext([_tenantService.CloudConfiguration.ArmEnvironment.DefaultScope]), cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpClient = _httpClientFactory.CreateClient(nameof(PostgreSQLUsageChecker));
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
