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

    // ARM metadata entity name whose supportedValues[] carry the per-recommendation-type
    // linkage we surface to callers (id, displayName, category, impact, resourceType, subCategory).
    // The other entities (recommendationCategory, recommendationImpact, supportedResourceType)
    // only enumerate dimension labels and are intentionally not surfaced here — that's a
    // separate concern best handled by a future `metadata list` command.
    private const string RecommendationTypeEntityName = "recommendationType";

    // Impact ordering for client-side sorting. High → Medium → Low → (anything unknown).
    // Lookup is case-insensitive so we don't depend on ARM always returning "High" vs "high".
    private static readonly Dictionary<string, int> ImpactRank = new(StringComparer.OrdinalIgnoreCase)
    {
        ["High"] = 0,
        ["Medium"] = 1,
        ["Low"] = 2,
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
        string? resourceType,
        string? impact,
        string? category,
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

        // We only consume the `recommendationType` entity — its supportedValues[] entries are the
        // only ones that carry per-type linkage (category/impact/resourceType/subCategory).
        var typeEntity = apiResponse.Value.FirstOrDefault(e =>
            string.Equals(e.Name, RecommendationTypeEntityName, StringComparison.OrdinalIgnoreCase));

        var supportedValues = typeEntity?.Properties?.SupportedValues;
        if (supportedValues == null || supportedValues.Count == 0)
        {
            return [];
        }

        // Normalize filter inputs once. Empty/whitespace means "no filter on this dimension".
        var trimmedResourceType = string.IsNullOrWhiteSpace(resourceType) ? null : resourceType.Trim();
        var trimmedImpact = string.IsNullOrWhiteSpace(impact) ? null : impact.Trim();
        var trimmedCategory = string.IsNullOrWhiteSpace(category) ? null : category.Trim();

        var results = new List<RecommendationType>(supportedValues.Count);
        foreach (var value in supportedValues)
        {
            if (string.IsNullOrEmpty(value.Id))
            {
                continue;
            }

            // Apply client-side filters case-insensitively. Data volume here is small
            // (a few hundred recommendation types) so this is acceptable; server-side
            // filtering on the ARM metadata endpoint is not currently supported.
            if (trimmedResourceType != null &&
                !string.Equals(value.SupportedResourceType, trimmedResourceType, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (trimmedImpact != null &&
                !string.Equals(value.RecommendationImpact, trimmedImpact, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (trimmedCategory != null &&
                !string.Equals(value.RecommendationCategory, trimmedCategory, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            results.Add(new RecommendationType(
                Id: value.Id,
                DisplayName: value.DisplayName ?? value.Id,
                Category: value.RecommendationCategory,
                Impact: value.RecommendationImpact,
                ResourceType: value.SupportedResourceType,
                SubCategory: value.RecommendationSubCategory));
        }

        // Sort by impact (High → Medium → Low → Unknown), then by displayName for stable ordering.
        // This surfaces the most important recommendations first, which matches the meeting outcome
        // (Sachin: "return all recommendations for that resource type, sorted by impact level").
        return [.. results
            .OrderBy(r => ImpactRank.TryGetValue(r.Impact ?? string.Empty, out var rank) ? rank : int.MaxValue)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)];
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
