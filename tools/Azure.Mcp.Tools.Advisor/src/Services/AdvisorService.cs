// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Json;
using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IHttpClientFactory httpClientFactory,
    ILogger<AdvisorService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IAdvisorService
{
    private const string AdvisorMetadataApiVersion = "2025-01-01";

    // Map friendly user-facing filter values to the actual entity names returned by the
    // ARM Advisor metadata API (case-insensitive). Friendly values used in CLI/MCP options;
    // ARM entity names verified at /providers/Microsoft.Advisor/metadata?api-version=2025-01-01.
    private static readonly Dictionary<string, string> FilterToEntityName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["recommendationType"] = "recommendationType",
        ["category"] = "recommendationCategory",
        ["impact"] = "recommendationImpact",
        ["resourceType"] = "supportedResourceType",
    };

    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<AdvisorService> _logger = logger;

    public async Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteResourceQueryAsync(
            "Microsoft.Advisor/recommendations",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToAdvisorRecommendationModel,
            tableName: "advisorresources",
            cancellationToken: cancellationToken);
    }

    public async Task<List<RecommendationType>> ListRecommendationTypesAsync(
        string? tenant,
        string? filter,
        CancellationToken cancellationToken = default)
    {
        var managementEndpoint = _tenantService.CloudConfiguration.ArmEnvironment.Endpoint
            ?? throw new InvalidOperationException("Management endpoint is not configured.");

        var token = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);

        var requestUri = new Uri(managementEndpoint, $"/providers/Microsoft.Advisor/metadata?api-version={AdvisorMetadataApiVersion}");

        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            // Read the response body for diagnostic logging only; do NOT include it in the thrown
            // exception message because that message is surfaced to the caller and the body may
            // contain verbose ARM error details we don't want to leak to the user.
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            const int maxLoggedBodyLength = 2048;
            var truncatedBody = body.Length > maxLoggedBodyLength
                ? body[..maxLoggedBodyLength] + "... [truncated]"
                : body;
            _logger.LogError(
                "Advisor metadata API returned non-success. Status: {Status}, Reason: {Reason}, Body: {Body}",
                (int)response.StatusCode, response.ReasonPhrase, truncatedBody);
            throw new HttpRequestException(
                $"Advisor metadata API returned {(int)response.StatusCode} {response.ReasonPhrase}.",
                inner: null,
                response.StatusCode);
        }

        var apiResponse = await response.Content.ReadFromJsonAsync(
            AdvisorJsonContext.Default.RecommendationMetadataApiResponse,
            cancellationToken);

        if (apiResponse?.Value == null)
        {
            return [];
        }

        IEnumerable<RecommendationMetadataEntity> entities = apiResponse.Value;
        if (!string.IsNullOrWhiteSpace(filter))
        {
            // Translate friendly value (e.g. 'category') to actual ARM entity name (e.g. 'recommendationCategory').
            // If the caller passed the raw ARM name, it still matches via the case-insensitive Equals fallback below.
            var entityName = FilterToEntityName.TryGetValue(filter, out var mapped) ? mapped : filter;
            entities = entities.Where(e =>
                string.Equals(e.Name, entityName, StringComparison.OrdinalIgnoreCase));
        }

        var results = new List<RecommendationType>();
        foreach (var entity in entities)
        {
            var supportedValues = entity.Properties?.SupportedValues;
            if (supportedValues == null)
            {
                continue;
            }

            foreach (var value in supportedValues)
            {
                if (string.IsNullOrEmpty(value.Id))
                {
                    continue;
                }

                results.Add(new RecommendationType(
                    Id: value.Id,
                    DisplayName: value.DisplayName ?? value.Id));
            }
        }

        return results;
    }

    private static Recommendation ConvertToAdvisorRecommendationModel(JsonElement item)
    {
        Models.RecommendationData? advisorRecommendation = Models.RecommendationData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse Advisor recommendation data");

        return new(
            ResourceId: advisorRecommendation.Properties?.ResourceMetadata?.ResourceId ?? "Unknown",
            RecommendationText: advisorRecommendation.Properties?.ShortDescription?.Problem ?? "Unknown",
            Category: advisorRecommendation.Properties?.Category ?? "Unknown");
    }
}
