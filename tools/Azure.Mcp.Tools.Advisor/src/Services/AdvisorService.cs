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
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
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

    internal const string GroupByRecommendationType = "recommendation-type";
    internal const string GroupByCategory = "category";
    internal const string GroupByImpact = "impact";
    internal const string GroupByResourceType = "resource-type";

    internal static readonly IReadOnlyList<string> AllowedGroupBy =
    [
        GroupByRecommendationType,
        GroupByCategory,
        GroupByImpact,
        GroupByResourceType,
    ];

    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ISubscriptionService _advisorSubscriptionService = subscriptionService;
    private readonly ILogger<AdvisorService> _logger = logger;

    public async Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        RecommendationFilters? filters = null,
        int top = 50,
        CancellationToken cancellationToken = default)
    {
        var additionalFilter = BuildAdditionalFilter(filters);

        return await ExecuteResourceQueryAsync(
            "Microsoft.Advisor/recommendations",
            resourceGroup,
            subscription,
            retryPolicy,
            ConvertToAdvisorRecommendationModel,
            tableName: "advisorresources",
            additionalFilter: additionalFilter,
            limit: top,
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
            // Read the error response body for diagnostic logging only; do NOT include it in the
            // thrown exception message because that message is surfaced to the caller and the body
            // may contain verbose ARM error details we don't want to leak to the user.
            //
            // NOTE: This truncation applies ONLY to the *error* body we write to the log. It does
            // not affect the successful response below, which is deserialized in full via
            // ReadFromJsonAsync (no size cap), so all recommendation types are always returned.
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            const int maxLoggedErrorBodyLength = 2048;
            var truncatedBody = body.Length > maxLoggedErrorBodyLength
                ? body[..maxLoggedErrorBodyLength] + "... [truncated]"
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

    public async Task<RecommendationSummary> SummarizeRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        string groupBy,
        RecommendationFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var subscriptionResource = await _advisorSubscriptionService.GetSubscription(subscription, null, retryPolicy, cancellationToken);
        var allTenants = await TenantService.GetTenants(cancellationToken);
        var tenantResource = allTenants.FirstOrDefault(t => t.Data.TenantId == subscriptionResource!.Data.TenantId)
            ?? throw new InvalidOperationException($"No accessible tenant found for subscription '{subscription}'");

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgExists = await subscriptionResource!.GetResourceGroups().ExistsAsync(resourceGroup, cancellationToken);
            if (!rgExists.Value)
            {
                throw new KeyNotFoundException(
                    $"Resource group '{resourceGroup}' does not exist in subscription '{subscriptionResource.Data.SubscriptionId}'");
            }
        }

        var query = BuildSummarizeQuery(groupBy, resourceGroup, filters);
        var queryContent = new ResourceQueryContent(query)
        {
            Subscriptions = { subscriptionResource!.Data.SubscriptionId }
        };

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);

        var allGroups = new List<RecommendationGroup>();
        if (result?.Count > 0)
        {
            using var jsonDocument = JsonDocument.Parse(result.Data);
            var dataArray = jsonDocument.RootElement;
            if (dataArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataArray.EnumerateArray())
                {
                    var key = item.TryGetProperty("key", out var keyProp) && keyProp.ValueKind == JsonValueKind.String
                        ? keyProp.GetString() ?? "Unknown"
                        : "Unknown";
                    var count = item.TryGetProperty("count_", out var countProp) ? countProp.GetInt64() : 0;
                    allGroups.Add(new RecommendationGroup(key, (int)count));
                }
            }
        }

        var totalRecommendations = allGroups.Sum(g => g.Count);

        return new RecommendationSummary(
            GroupBy: groupBy,
            TotalRecommendations: totalRecommendations,
            Groups: allGroups);
    }

    internal static string BuildSummarizeQuery(string groupBy, string? resourceGroup, RecommendationFilters? filters)
    {
        var query = "advisorresources | where type =~ 'Microsoft.Advisor/recommendations'";

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            query += $" and resourceGroup =~ '{EscapeKqlString(resourceGroup)}'";
        }

        var additionalFilter = BuildAdditionalFilter(filters);
        if (!string.IsNullOrEmpty(additionalFilter))
        {
            query += $" and {additionalFilter}";
        }

        var summarizeField = MapGroupByToKqlField(groupBy);
        query += $" | summarize count() by key={summarizeField}";
        // Push 'Unknown' to the end regardless of count so real categories are surfaced first.
        query += " | order by iff(key == 'Unknown', 1, 0) asc, count_ desc, key asc";

        return query;
    }

    internal static string MapGroupByToKqlField(string groupBy) => groupBy.ToLowerInvariant() switch
    {
        GroupByCategory =>
            "iff(isempty(tostring(properties.category)), 'Unknown', tostring(properties.category))",
        GroupByImpact =>
            "iff(isempty(tostring(properties.impact)), 'Unknown', tostring(properties.impact))",
        GroupByRecommendationType =>
            "iff(isempty(tostring(properties.shortDescription.problem)), 'Unknown', tostring(properties.shortDescription.problem))",
        GroupByResourceType =>
            "iff(isempty(extract(@'/providers/([^/]+/[^/]+)', 1, tostring(properties.resourceMetadata.resourceId))), 'Unknown', " +
            "extract(@'/providers/([^/]+/[^/]+)', 1, tostring(properties.resourceMetadata.resourceId)))",
        _ => throw new ArgumentException(
            $"Unsupported group-by value '{groupBy}'. Allowed values: {string.Join(", ", AllowedGroupBy)}.",
            nameof(groupBy)),
    };

    internal static string? BuildAdditionalFilter(RecommendationFilters? filters)
    {
        // Advisor surfaces recommendations in several lifecycle states (e.g. 'New', 'Dismissed', 'Postponed').
        // Only 'New' recommendations are active and actionable, so we always constrain results to these and
        // never expose dismissed or postponed noise in lists or summaries.
        var clauses = new List<string> { ActiveRecommendationClause };

        if (filters is not null)
        {
            if (!string.IsNullOrWhiteSpace(filters.Category))
            {
                clauses.Add($"tostring(properties.category) =~ '{SanitizeForKql(filters.Category)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.Impact))
            {
                clauses.Add($"tostring(properties.impact) =~ '{SanitizeForKql(filters.Impact)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.ResourceType))
            {
                clauses.Add($"tostring(properties.resourceMetadata.resourceId) contains '{SanitizeForKql(filters.ResourceType)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.Resource))
            {
                clauses.Add($"tostring(properties.resourceMetadata.resourceId) contains '{SanitizeForKql(filters.Resource)}'");
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                clauses.Add($"tostring(properties.shortDescription.problem) contains '{SanitizeForKql(filters.Search)}'");
            }
        }

        return string.Join(" and ", clauses);
    }

    // KQL clause that restricts results to active ('New') recommendations only.
    internal const string ActiveRecommendationClause = "tostring(properties.recommendationStatus) =~ 'New'";

    private static string SanitizeForKql(string value) => EscapeKqlString(value.Replace("|", string.Empty));

    internal static Recommendation ConvertToAdvisorRecommendationModel(JsonElement item)
    {
        Models.RecommendationData? advisorRecommendation = Models.RecommendationData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse Advisor recommendation data");

        var resourceId = advisorRecommendation.Properties?.ResourceMetadata?.ResourceId ?? "Unknown";

        return new(
            ResourceId: resourceId,
            RecommendationText: advisorRecommendation.Properties?.ShortDescription?.Problem ?? "Unknown",
            Category: advisorRecommendation.Properties?.Category ?? "Unknown",
            Impact: advisorRecommendation.Properties?.Impact,
            ImpactedResourceType: ParseImpactedResourceType(resourceId));
    }

    internal static string? ParseImpactedResourceType(string? resourceId)
    {
        if (string.IsNullOrEmpty(resourceId))
        {
            return null;
        }

        var segments = resourceId.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? ns = null;
        var typeParts = new List<string>();

        for (var i = 0; i < segments.Length; i++)
        {
            if (!string.Equals(segments[i], "providers", StringComparison.OrdinalIgnoreCase) || i + 2 >= segments.Length)
            {
                continue;
            }

            ns = segments[i + 1];
            typeParts.Clear();
            typeParts.Add(segments[i + 2]);

            for (var j = i + 4; j < segments.Length; j += 2)
            {
                typeParts.Add(segments[j]);
            }

            break;
        }

        return ns is null ? null : $"{ns}/{string.Join('/', typeParts)}";
    }
}
