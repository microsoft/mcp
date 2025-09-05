// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Azure.Core;
using Azure.Mcp.Core.Services.Http;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Quota.Services.Util.Usage;

public class PostgreSQLUsageChecker(TokenCredential credential, string subscriptionId, ILogger<PostgreSQLUsageChecker> logger, IHttpClientService httpClientService) : AzureUsageChecker(credential, subscriptionId, logger)
{
    private readonly IHttpClientService _httpClientService = httpClientService;

    public override async Task<List<UsageInfo>> GetUsageForLocationAsync(string location)
    {
        try
        {
            var requestUrl = $"{managementEndpoint}/subscriptions/{SubscriptionId}/providers/Microsoft.DBforPostgreSQL/locations/{location}/resourceType/flexibleServers/usages?api-version=2023-06-01-preview";
            using var rawResponse = await GetQuotaByUrlAsync(requestUrl);

            if (rawResponse?.RootElement.TryGetProperty("value", out var valueElement) != true)
            {
                return [];
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

                result.Add(new UsageInfo(name, limit, used, unit));
            }

            return result;
        }
        catch (Exception error)
        {
            throw new InvalidOperationException("Failed to fetch PostgreSQL quotas. Please check your subscription permissions and network connectivity.", error);
        }
    }

    protected async Task<JsonDocument?> GetQuotaByUrlAsync(string requestUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await Credential.GetTokenAsync(new TokenRequestContext([$"{managementEndpoint}/.default"]), cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClientService.DefaultClient.SendAsync(request, cancellationToken);

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
