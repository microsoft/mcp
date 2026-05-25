// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<AdvisorService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IAdvisorService
{
    private readonly ILogger<AdvisorService> _logger = logger;

    public async Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        RecommendationFilters? filters = null,
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
            cancellationToken: cancellationToken);
    }

    public async Task<RecommendationSummary> SummarizeRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        string groupBy,
        int top,
        RecommendationFilters? filters = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var listResults = await ListRecommendationsAsync(
            subscription,
            resourceGroup,
            retryPolicy,
            filters,
            cancellationToken);

        var groups = RecommendationAggregator.Aggregate(listResults.Results, groupBy, top);

        return new RecommendationSummary(
            GroupBy: groupBy,
            Top: top,
            TotalRecommendations: listResults.Results.Count,
            AreResultsTruncated: listResults.AreResultsTruncated,
            Groups: groups);
    }

    internal static string? BuildAdditionalFilter(RecommendationFilters? filters)
    {
        if (filters is null)
        {
            return null;
        }

        var clauses = new List<string>();

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
            // Match against the ARM id, which contains '/providers/{namespace}/{type}/...'.
            // Case-insensitive contains lets callers pass either the full type
            // ('Microsoft.Storage/storageAccounts') or a fragment ('storageAccounts', 'Storage').
            clauses.Add($"tolower(tostring(properties.resourceMetadata.resourceId)) contains tolower('{SanitizeForKql(filters.ResourceType)}')");
        }

        if (!string.IsNullOrWhiteSpace(filters.Resource))
        {
            clauses.Add($"tolower(tostring(properties.resourceMetadata.resourceId)) contains tolower('{SanitizeForKql(filters.Resource)}')");
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            clauses.Add($"tolower(tostring(properties.shortDescription.problem)) contains tolower('{SanitizeForKql(filters.Search)}')");
        }

        return clauses.Count == 0 ? null : string.Join(" and ", clauses);
    }

    // Strip the KQL pipe operator before escaping. BaseAzureResourceService rejects
    // any additionalFilter containing '|' as an injection guard, so allowing it
    // through would surface as a confusing ArgumentException to the caller. Real
    // Advisor categories/impacts never contain '|', and dropping it from free-text
    // search input is a safer default than failing the whole query.
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
