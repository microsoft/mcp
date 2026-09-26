// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Validation;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAdvisorService
{
    private const string RecommendationUpdateApiVersion = "2026-03-01-preview";
    private const string RetirementDateProperty =
        "properties.sourceProperties.serviceRetirement.retirementDate";
    private const string TrackingIdsProperty =
        "properties.sourceProperties.serviceRetirement.serviceHealth.trackingIds";
    private const int MetadataPageSize = 1000;

    // Recommendation instances are not localized per request, so the metadata join always uses the
    // invariant English metadata to keep the enriched fields deterministic.
    internal const string MetadataJoinLanguage = "en";

    private static readonly Dictionary<string, int> ImpactRank = new(StringComparer.OrdinalIgnoreCase)
    {
        ["High"] = 0,
        ["Medium"] = 1,
        ["Low"] = 2,
    };

    public async Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string? subscription,
        string? resourceGroup,
        RecommendationFilters? filters = null,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(top, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(top, 100);

        var serviceGroup = string.IsNullOrWhiteSpace(filters?.ServiceGroup) ? null : filters!.ServiceGroup!.Trim();
        var isServiceGroupScope = serviceGroup is not null;

        // Resolve the subscription up front for subscription scope so the metadata join uses the same tenant
        // and scope validation surfaces invalid subscriptions or resource groups. Service Group scope is
        // subscription-independent and runs against the tenant directly.
        SubscriptionResource? subscriptionResource = null;
        string? metadataTenant = tenant;
        if (!isServiceGroupScope)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
            subscriptionResource = await AzureService.GetSubscription(
                subscription,
                tenant,
                cancellationToken: cancellationToken);
            metadataTenant = subscriptionResource.Data.TenantId.ToString();
        }

        Dictionary<string, RecommendationMetadata>? metadataByTypeId =
            await ResolveMetadataFilterMatchesAsync(filters, metadataTenant, cancellationToken);

        if (metadataByTypeId is { Count: 0 })
        {
            if (!isServiceGroupScope)
            {
                // Validate the resource group so an invalid one fails instead of returning an empty success.
                await EnsureResourceGroupExistsAsync(subscriptionResource!, resourceGroup, cancellationToken);
            }

            return new([], false);
        }

        var scope = isServiceGroupScope
            ? RecommendationQueryScope.ForServiceGroup(serviceGroup!)
            : RecommendationQueryScope.ForSubscription(
                subscriptionResource!.Data.SubscriptionId
                    ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID."),
                resourceGroup);

        var prioritized = IsPrioritized(filters);
        var predicates = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            recommendationTypeIds: metadataByTypeId?.Keys,
            scope: scope);

        // A single tenant-scoped query joins recommendations with the metadata catalog and overrides the
        // type-level fields server-side, so no follow-up metadata call or client-side merge is needed.
        var query = BuildRecommendationListQuery(
            scope,
            predicates,
            prioritized,
            top,
            MetadataJoinLanguage);

        var recommendations = await ExecuteRecommendationListQueryAsync(
            query,
            top,
            subscriptionResource,
            tenant,
            cancellationToken);

        if (recommendations.Results.Count == 0 && !isServiceGroupScope)
        {
            // Preserve the resource-group-not-found signal that the subscription-scoped query would produce.
            await EnsureResourceGroupExistsAsync(subscriptionResource!, resourceGroup, cancellationToken);
        }

        return recommendations;
    }

    // Prioritized mode surfaces only criticality-scored recommendations, ranked by metadata priority score.
    internal static bool IsPrioritized(RecommendationFilters? filters) =>
        filters?.Prioritized == true;

    internal static string BuildRecommendationListQuery(
        RecommendationQueryScope scope,
        string? predicates,
        bool prioritized,
        int limit,
        string language)
    {
        var query = "advisorresources | where type =~ 'Microsoft.Advisor/recommendations'";

        if (scope.SubscriptionId is { } subscriptionId)
        {
            query += $" and subscriptionId =~ '{RecommendationQueryBuilder.EscapeKqlString(subscriptionId)}'";
        }

        if (scope.ResourceGroup is { } resourceGroup)
        {
            query += $" and resourceGroup =~ '{RecommendationQueryBuilder.EscapeKqlString(resourceGroup)}'";
        }

        if (!string.IsNullOrEmpty(predicates))
        {
            query += $" and {predicates}";
        }

        if (prioritized)
        {
            // Only recommendations that carry criticality scoring participate in the ranked view.
            query += " and isnotempty(tostring(properties.criticality)) and isnotnull(properties.criticalityScore)";
        }

        query += " | extend joinTypeId = tolower(tostring(properties.recommendationTypeId))";
        query += BuildMetadataJoinClause(language);
        query += MetadataOverrideClause;

        if (prioritized)
        {
            // Rank by the recommendation type's metadata priority score; break ties on equal scores by business
            // impact (High -> Medium -> Low), then keep each type's instances contiguous, then order by criticality
            // within the type. Ordering runs before the projection so the metadata-only sort keys survive; missing
            // scores/impact sort last, and id is the final tiebreaker for a fully deterministic result.
            query += " | extend metadataImpactRank = case(" +
                "tolower(metadataImpact) == 'high', 0, " +
                "tolower(metadataImpact) == 'medium', 1, " +
                "tolower(metadataImpact) == 'low', 2, 3)";
            query += " | order by metadataPriorityScore desc, metadataImpactRank asc, joinTypeId asc," +
                " todouble(properties.criticalityScore) desc, id asc";
        }

        query += " | project id, name, type, properties";

        return query + $" | limit {limit}";
    }

    // Left-outer join to the English recommendation-type catalog, projecting the fields the list view overrides.
    private static string BuildMetadataJoinClause(string language) =>
        " | join kind=leftouter (" +
        " advisorresources" +
        " | where type =~ 'microsoft.advisor/metadata'" +
        $" | where tostring(properties.language) =~ '{RecommendationQueryBuilder.EscapeKqlString(language.Trim())}'" +
        " | extend joinTypeId = tolower(tostring(properties.recommendationTypeId))" +
        " | project joinTypeId," +
        " metadataCategory = tostring(properties.recommendationCategory)," +
        " metadataImpact = tostring(properties.recommendationImpact)," +
        " metadataSubCategory = tostring(properties.recommendationSubCategory)," +
        " metadataDisplayName = tostring(properties.displayName)," +
        " metadataLabel = tostring(properties.label)," +
        " metadataDescription = tostring(properties.detailedDescription)," +
        " metadataLearnMoreLink = tostring(properties.learnMoreLink)," +
        " metadataPotentialBenefits = tostring(properties.potentialBenefits)," +
        " metadataRetirementDate = tostring(properties.sourceProperties.serviceRetirement.retirementDate)," +
        " metadataRetirementFeatureName = tostring(properties.sourceProperties.serviceRetirement.retirementFeatureName)," +
        " metadataPriorityScore = todouble(properties.priorityScore)" +
        " ) on joinTypeId";

    // Reapplies the field precedence the former client-side merge used, then rewrites properties in place.
    private const string MetadataOverrideClause =
        " | extend instanceLabel = tostring(properties.label), instanceSourceSystem = tostring(properties.sourceSystem)" +
        " | extend mergedLabel = iff(isempty(instanceSourceSystem)," +
        " iff(isnotempty(metadataLabel), metadataLabel, instanceLabel)," +
        " iff(isnotempty(instanceLabel), instanceLabel, metadataLabel))" +
        " | extend hasExtendedAdditions = isnotempty(metadataSubCategory) or isnotempty(metadataRetirementDate) or isnotempty(metadataRetirementFeatureName)" +
        " | extend mergedExtendedProperties = bag_merge(" +
        " iff(isnotempty(metadataSubCategory), pack('recommendationSubCategory', metadataSubCategory), dynamic({}))," +
        " iff(isnotempty(metadataRetirementDate), pack('retirementDate', metadataRetirementDate), dynamic({}))," +
        " iff(isnotempty(metadataRetirementFeatureName), pack('retirementFeatureName', metadataRetirementFeatureName), dynamic({}))," +
        " coalesce(properties.extendedProperties, dynamic({})))" +
        " | extend metadataOverrides = bag_merge(" +
        " iff(isnotempty(metadataCategory), pack('category', metadataCategory), dynamic({}))," +
        " iff(isnotempty(metadataImpact), pack('impact', metadataImpact), dynamic({}))," +
        " iff(isnotempty(metadataDescription), pack('description', metadataDescription), dynamic({}))," +
        " iff(isnotempty(metadataLearnMoreLink), pack('learnMoreLink', metadataLearnMoreLink), dynamic({}))," +
        " iff(isnotempty(metadataPotentialBenefits), pack('potentialBenefits', metadataPotentialBenefits), dynamic({}))," +
        " iff(isnotempty(mergedLabel), pack('label', mergedLabel), dynamic({}))," +
        " iff(isnull(properties.shortDescription) and isnotempty(metadataDisplayName)," +
        " pack('shortDescription', pack('problem', metadataDisplayName, 'solution', metadataDisplayName)), dynamic({}))," +
        " iff(hasExtendedAdditions, pack('extendedProperties', mergedExtendedProperties), dynamic({})))" +
        " | extend properties = bag_merge(metadataOverrides, properties)";

    private async Task<ResourceQueryResults<Recommendation>> ExecuteRecommendationListQueryAsync(
        string query,
        int limit,
        SubscriptionResource? subscriptionResource,
        string? tenant,
        CancellationToken cancellationToken)
    {
        // The query joins tenant-level metadata, so it runs at tenant scope for both subscription and
        // Service Group scopes; the subscription filter is expressed inside the query itself.
        var tenantResource = subscriptionResource is not null
            ? await GetTenantResourceForSubscriptionAsync(subscriptionResource, cancellationToken)
            : await GetTenantResourceAsync(tenant, cancellationToken);
        var queryContent = new ResourceQueryContent(query);

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);

        return ParseRecommendationListResult(
            result?.Data,
            result?.ResultTruncated == ResultTruncated.True,
            result?.SkipToken);
    }

    internal static ResourceQueryResults<Recommendation> ParseRecommendationListResult(
        BinaryData? data,
        bool isTruncated,
        string? skipToken)
    {
        var results = new List<Recommendation>();
        if (data is not null)
        {
            using var jsonDocument = JsonDocument.Parse(data);
            if (jsonDocument.RootElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonDocument.RootElement.EnumerateArray())
                {
                    results.Add(ConvertToAdvisorRecommendationModel(item));
                }
            }
        }

        return new(results, isTruncated || !string.IsNullOrEmpty(skipToken));
    }

    private async Task<TenantResource> GetTenantResourceForSubscriptionAsync(
        SubscriptionResource subscriptionResource,
        CancellationToken cancellationToken)
    {
        var allTenants = await AzureService.GetTenants(cancellationToken);
        return allTenants.FirstOrDefault(t => t.Data.TenantId == subscriptionResource.Data.TenantId)
            ?? throw new InvalidOperationException(
                $"No accessible tenant found for subscription '{subscriptionResource.Data.SubscriptionId}'.");
    }

    public async Task<Recommendation> UpdateRecommendationAsync(
        string subscription,
        string recommendationId,
        RecommendationStatus recommendationStatus,
        DateTimeOffset? postponedUntilDateTime = null,
        RecommendationDismissReason? recommendationDismissReason = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(recommendationId), recommendationId));
        RecommendationStateUpdateValidator.Validate(
            recommendationStatus,
            postponedUntilDateTime,
            recommendationDismissReason);
        recommendationDismissReason = RecommendationStateUpdateValidator.ResolveDismissReason(
            recommendationStatus,
            recommendationDismissReason);

        var subscriptionResource = await AzureService.GetSubscription(
            subscription,
            tenant,
            cancellationToken);
        var subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID.");
        var managementEndpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint;
        var accessToken = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var relativePath =
            $"/subscriptions/{Uri.EscapeDataString(subscriptionId)}/providers/Microsoft.Advisor/recommendations/" +
            $"{Uri.EscapeDataString(recommendationId.Trim())}?api-version={RecommendationUpdateApiVersion}";
        var requestUri = new Uri(managementEndpoint, relativePath);
        var properties = new Models.RecommendationStatePatchProperties(
            recommendationStatus,
            recommendationStatus == RecommendationStatus.Postponed ? postponedUntilDateTime : null,
            recommendationStatus == RecommendationStatus.Dismissed ? recommendationDismissReason : null);

        using var client = AzureService.GetClient();
        using var response = await SendRecommendationUpdateAsync(
            client,
            requestUri,
            accessToken.Token,
            properties,
            cancellationToken);

        if (response.IsError)
        {
            throw CreateRecommendationUpdateException(response);
        }

        using var document = JsonDocument.Parse(response.Content.ToStream());

        return ConvertUpdateResponseToAdvisorRecommendationModel(document.RootElement);
    }

    private static async Task<Response> SendRecommendationUpdateAsync(
        HttpClient client,
        Uri requestUri,
        string accessToken,
        Models.RecommendationStatePatchProperties properties,
        CancellationToken cancellationToken)
    {
        var clientOptions = AddDefaultPolicies(new ArmClientOptions());
        clientOptions.Transport = new HttpClientTransport(client);

        var pipeline = HttpPipelineBuilder.Build(clientOptions);
        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Patch;
        request.Uri.Reset(requestUri);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Content-Type", "application/json");
        request.Content = RequestContent.Create(JsonSerializer.SerializeToUtf8Bytes(
            new Models.RecommendationStatePatchRequest(properties),
            AdvisorJsonContext.Default.RecommendationStatePatchRequest));

        return await pipeline.SendRequestAsync(request, cancellationToken);
    }

    private static async Task EnsureResourceGroupExistsAsync(
        SubscriptionResource subscriptionResource,
        string? resourceGroup,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resourceGroup))
        {
            return;
        }

        var normalizedResourceGroup = resourceGroup.Trim();
        var rgExists = await subscriptionResource.GetResourceGroups().ExistsAsync(normalizedResourceGroup, cancellationToken);
        if (!rgExists.Value)
        {
            throw new KeyNotFoundException(
                $"Resource group '{normalizedResourceGroup}' does not exist in subscription '{subscriptionResource.Data.SubscriptionId}'");
        }
    }

    internal static bool HasMetadataOnlyFilters(RecommendationFilters? filters) =>
        !string.IsNullOrWhiteSpace(filters?.SubCategory) ||
        filters?.TrackingIds?.Any(id => !string.IsNullOrWhiteSpace(id)) == true ||
        filters?.RetirementDate is not null ||
        !string.IsNullOrWhiteSpace(filters?.RetirementDateOperator);

    // Resolve these filters against metadata so category and impact match the enriched values returned.
    // This adds a catalog lookup and can produce a broad recommendationTypeId predicate.
    internal static bool HasMetadataFilters(RecommendationFilters? filters) =>
        HasMetadataOnlyFilters(filters) ||
            (!string.IsNullOrWhiteSpace(filters?.Category) ||
            !string.IsNullOrWhiteSpace(filters?.Impact) ||
            !string.IsNullOrWhiteSpace(filters?.ResourceType));

    /// <summary>
    /// Resolves metadata-backed filters against metadata first and returns matching recommendation type IDs.
    /// Resource and search filters remain predicates on recommendation instances.
    /// </summary>
    private async Task<Dictionary<string, RecommendationMetadata>?> ResolveMetadataFilterMatchesAsync(
        RecommendationFilters? filters,
        string? tenant,
        CancellationToken cancellationToken)
    {
        if (!HasMetadataFilters(filters))
        {
            return null;
        }

        var matchingMetadata = await ListAllRecommendationMetadataAsync(
            MetadataJoinLanguage,
            new RecommendationMetadataFilters(
                ResourceType: filters!.ResourceType,
                Impact: filters.Impact,
                Category: filters.Category,
                SubCategory: filters!.SubCategory,
                TrackingIds: filters.TrackingIds,
                RetirementDateOperator: filters.RetirementDateOperator,
                RetirementDate: filters.RetirementDate),
            tenant,
            cancellationToken);

        return BuildMetadataLookup(matchingMetadata);
    }

    internal static Dictionary<string, RecommendationMetadata> BuildMetadataLookup(
        IEnumerable<RecommendationMetadata> metadata)
    {
        var lookup = new Dictionary<string, RecommendationMetadata>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in metadata)
        {
            if (!string.IsNullOrWhiteSpace(entry.RecommendationTypeId))
            {
                lookup[entry.RecommendationTypeId] = entry;
            }
        }

        return lookup;
    }

    private static string FormatKqlStringList(IEnumerable<string> values) =>
        string.Join(", ", values.Select(value => $"'{RecommendationQueryBuilder.SanitizeForKql(value)}'"));

    private static List<string> NormalizeFilterValues(IEnumerable<string>? values) =>
        values is null
            ? []
            : [.. values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)];

    public async Task<ResourceQueryResults<RecommendationMetadata>> ListRecommendationMetadataAsync(
        string language,
        RecommendationMetadataFilters? filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var query = BuildMetadataListQuery(language, filters);
        var tenantResource = await GetTenantResourceAsync(cancellationToken);
        var result = await ExecuteMetadataPageAsync(tenantResource, query, null, cancellationToken);

        return new(
            SortMetadata(result.Metadata),
            result.IsTruncated || !string.IsNullOrEmpty(result.SkipToken));
    }

    private async Task<List<RecommendationMetadata>> ListAllRecommendationMetadataAsync(
        string language,
        RecommendationMetadataFilters? filters,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var query = BuildMetadataListQuery(language, filters);
        var tenantResource = await GetTenantResourceAsync(tenant, cancellationToken);
        var results = new List<RecommendationMetadata>();
        results.AddRange(await CollectMetadataPagesAsync(
            (skipToken, token) => ExecuteMetadataPageAsync(
                tenantResource,
                query,
                skipToken,
                token),
            cancellationToken));

        return SortMetadata(BuildMetadataLookup(results).Values);
    }

    internal static async Task<List<RecommendationMetadata>> CollectMetadataPagesAsync(
        Func<string?, CancellationToken, Task<(List<RecommendationMetadata> Metadata, string? SkipToken, bool IsTruncated)>> getPage,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(getPage);

        var results = new List<RecommendationMetadata>();
        var seenSkipTokens = new HashSet<string>(StringComparer.Ordinal);
        string? skipToken = null;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await getPage(skipToken, cancellationToken);
            results.AddRange(page.Metadata);
            skipToken = page.SkipToken;

            if (page.IsTruncated && string.IsNullOrEmpty(skipToken))
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph truncated Advisor metadata results without returning a continuation token.");
            }

            if (!string.IsNullOrEmpty(skipToken) && !seenSkipTokens.Add(skipToken))
            {
                throw new InvalidOperationException(
                    "Azure Resource Graph returned a repeated continuation token while paging Advisor metadata.");
            }
        }
        while (!string.IsNullOrEmpty(skipToken));

        return results;
    }

    private static async Task<(List<RecommendationMetadata> Metadata, string? SkipToken, bool IsTruncated)> ExecuteMetadataPageAsync(
        TenantResource tenantResource,
        string query,
        string? skipToken,
        CancellationToken cancellationToken)
    {
        var queryContent = new ResourceQueryContent(query)
        {
            Options = new ResourceQueryRequestOptions
            {
                Top = MetadataPageSize,
                SkipToken = skipToken,
            },
        };

        var response = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        var result = response.Value;
        if (result == null || result.Count == 0)
        {
            return new([], result?.SkipToken, result?.ResultTruncated == ResultTruncated.True);
        }

        return new(
            ParseMetadata(result.Data),
            result.SkipToken,
            result.ResultTruncated == ResultTruncated.True);
    }

    private static List<RecommendationMetadata> ParseMetadata(BinaryData data)
    {
        var results = new List<RecommendationMetadata>();
        using var jsonDocument = JsonDocument.Parse(data);
        if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid recommendation metadata payload.");
        }

        foreach (var item in jsonDocument.RootElement.EnumerateArray())
        {
            results.Add(ConvertToRecommendationMetadataModel(item));
        }

        return results;
    }

    private static List<RecommendationMetadata> SortMetadata(IEnumerable<RecommendationMetadata> metadata) =>
        [.. metadata
            .OrderBy(r => ImpactRank.TryGetValue(r.Impact ?? string.Empty, out var rank) ? rank : int.MaxValue)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)];

    internal static string BuildMetadataListQuery(
        string language,
        RecommendationMetadataFilters? filters)
    {
        var query =
            "advisorresources " +
            "| where type =~ 'microsoft.advisor/metadata' " +
            $"| where tostring(properties.language) =~ '{EscapeKqlString(language.Trim())}'";

        if (!string.IsNullOrWhiteSpace(filters?.ResourceType))
        {
            query += $" | where tostring(properties.supportedResourceType) =~ '{EscapeKqlString(filters.ResourceType.Trim())}'";
        }

        if (!string.IsNullOrWhiteSpace(filters?.Impact))
        {
            query += $" | where tostring(properties.recommendationImpact) =~ '{EscapeKqlString(filters.Impact.Trim())}'";
        }

        var category = string.IsNullOrWhiteSpace(filters?.Category)
            ? null
            : filters.Category.Trim();
        var subCategory = string.IsNullOrWhiteSpace(filters?.SubCategory)
            ? null
            : filters.SubCategory.Trim();
        var trackingIds = NormalizeFilterValues(filters?.TrackingIds);
        var retirementDate = filters?.RetirementDate;
        var retirementDateOperator = string.IsNullOrWhiteSpace(filters?.RetirementDateOperator)
            ? null
            : filters.RetirementDateOperator.Trim();

        if ((retirementDate is null) != (retirementDateOperator is null))
        {
            throw new ArgumentException(
                "RetirementDate and RetirementDateOperator must be provided together.",
                nameof(filters));
        }

        var hasTrackingIdFilter = trackingIds.Count > 0;
        var hasRetirementDateFilter = retirementDate is not null && retirementDateOperator is not null;
        var hasServiceRetirementFilter = hasTrackingIdFilter || hasRetirementDateFilter;
        subCategory = ResolveServiceRetirementSubCategory(
            subCategory,
            hasServiceRetirementFilter);

        if (category is not null)
        {
            query += $" | where tostring(properties.recommendationCategory) =~ '{EscapeKqlString(category)}'";
        }

        if (subCategory is not null)
        {
            query += $" | where tostring(properties.recommendationSubCategory) =~ '{EscapeKqlString(subCategory)}'";
        }

        if (hasTrackingIdFilter)
        {
            query += $" | mv-expand trackingId = {TrackingIdsProperty}";
            query += $" | where tostring(trackingId) in~ ({FormatKqlStringList(trackingIds)})";
        }

        if (retirementDate is { } date && retirementDateOperator is not null)
        {
            query += $" | where isnotempty(tostring({RetirementDateProperty}))";
            query += $" | where startofday(todatetime({RetirementDateProperty})) " +
                $"{GetKqlComparisonOperator(retirementDateOperator)} " +
                $"datetime({date:yyyy-MM-dd})";
        }

        return query +
            " | project id, recommendationTypeId = tostring(properties.recommendationTypeId), properties" +
            " | order by id asc, recommendationTypeId asc";
    }

    private static string? ResolveServiceRetirementSubCategory(
        string? subCategory,
        bool hasServiceRetirementFilter)
    {
        var isServiceUpgradeAndRetirement = subCategory?.Equals(
            RecommendationMetadataFilters.ServiceRetirementSubCategory,
            StringComparison.OrdinalIgnoreCase) == true;

        if (hasServiceRetirementFilter && subCategory is not null && !isServiceUpgradeAndRetirement)
        {
            throw new ArgumentException(
                "When a subcategory is specified with tracking ID or retirement-date filters, it must be " +
                $"{RecommendationMetadataFilters.ServiceRetirementSubCategory}.",
                nameof(subCategory));
        }

        if (!hasServiceRetirementFilter && !isServiceUpgradeAndRetirement)
        {
            return subCategory;
        }

        return subCategory ?? RecommendationMetadataFilters.ServiceRetirementSubCategory;
    }

    private static string GetKqlComparisonOperator(string comparisonOperator) =>
        comparisonOperator.ToLowerInvariant() switch
        {
            "eq" => "==",
            "lt" => "<",
            "le" => "<=",
            "gt" => ">",
            "ge" => ">=",
            _ => throw new ArgumentOutOfRangeException(
                nameof(comparisonOperator),
                comparisonOperator,
                "Unsupported retirement-date comparison operator.")
        };

    internal static RecommendationMetadata ConvertToRecommendationMetadataModel(JsonElement item)
    {
        var data = Models.RecommendationMetadataData.FromJson(item)
            ?? throw new JsonException("Failed to parse Advisor recommendation metadata data.");

        var properties = data.Properties
            ?? throw new JsonException("Recommendation metadata record is missing its properties payload.");
        if (string.IsNullOrWhiteSpace(properties.RecommendationTypeId))
        {
            throw new JsonException("Recommendation metadata record is missing recommendationTypeId.");
        }

        RecommendationServiceRetirement? serviceRetirement = null;
        var retirement = properties.SourceProperties?.ServiceRetirement;
        if (retirement is not null)
        {
            serviceRetirement = new RecommendationServiceRetirement(
                RetirementDate: retirement.RetirementDate,
                RetirementFeatureName: retirement.RetirementFeatureName,
                TrackingIds: retirement.ServiceHealth?.TrackingIds,
                AshUrls: retirement.ServiceHealth?.AshUrls);
        }

        IReadOnlyList<RecommendationMetadataAction>? actions = null;
        if (properties.Actions is { Count: > 0 } actionList)
        {
            actions = actionList
                .Select(ConvertMetadataAction)
                .ToList();
        }

        return new RecommendationMetadata(
            RecommendationTypeId: properties.RecommendationTypeId,
            DisplayName: properties.DisplayName,
            Label: properties.Label,
            Category: properties.RecommendationCategory,
            SubCategory: properties.RecommendationSubCategory,
            Impact: properties.RecommendationImpact,
            PriorityScore: properties.PriorityScore,
            PotentialBenefits: properties.PotentialBenefits,
            DetailedDescription: properties.DetailedDescription,
            LearnMoreLink: properties.LearnMoreLink,
            SupportedResourceType: properties.SupportedResourceType,
            Scope: properties.RecommendationScope,
            DataSourceQuery: properties.RecommendationDataSourceQuery,
            ResourceSingularName: properties.ResourceMetadata?.Singular,
            ResourcePluralName: properties.ResourceMetadata?.Plural,
            Actions: actions,
            Language: properties.Language,
            LastRefreshed: properties.LastRefreshed,
            ServiceRetirement: serviceRetirement);
    }

    private static RecommendationMetadataAction ConvertMetadataAction(
        Models.RecommendationMetadataActionData action) =>
        new(
            ActionType: action.ActionType,
            Caption: action.Caption,
            DocumentLink: action.DocumentLink,
            BladeName: action.BladeName);

    private async Task<TenantResource> GetTenantResourceAsync(CancellationToken cancellationToken)
        => await GetTenantResourceAsync(null, cancellationToken);

    private async Task<TenantResource> GetTenantResourceAsync(
        string? tenant,
        CancellationToken cancellationToken)
    {
        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found.");
        }

        if (string.IsNullOrWhiteSpace(tenant))
        {
            return tenants[0];
        }

        var resolvedTenantId = await AzureService.ResolveTenantIdAsync(tenant, cancellationToken)
            ?? throw new InvalidOperationException($"Could not resolve tenant '{tenant}'.");
        var tenantId = Guid.Parse(resolvedTenantId);
        return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
            ?? throw new InvalidOperationException($"No accessible tenant found for tenant '{tenant}'.");
    }

    public async Task<RecommendationMetadata?> GetRecommendationMetadataAsync(
        string recommendationTypeId,
        string language,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recommendationTypeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var tenantResource = await GetTenantResourceAsync(cancellationToken);

        var query =
            "advisorresources " +
            "| where type =~ 'microsoft.advisor/metadata' " +
            $"and tostring(properties.recommendationTypeId) =~ '{EscapeKqlString(recommendationTypeId)}' " +
            $"and tostring(properties.language) =~ '{EscapeKqlString(language)}' " +
            "| limit 1";

        var queryContent = new ResourceQueryContent(query);

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        if (result == null || result.Count == 0)
        {
            return null;
        }

        using var jsonDocument = JsonDocument.Parse(result.Data);
        var dataArray = jsonDocument.RootElement;
        if (dataArray.ValueKind != JsonValueKind.Array || dataArray.GetArrayLength() == 0)
        {
            return null;
        }

        return ConvertToRecommendationMetadataModel(dataArray[0]);
    }

    internal static Recommendation ConvertToAdvisorRecommendationModel(JsonElement item)
    {
        var advisorRecommendation = Models.RecommendationData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse Advisor recommendation data");

        return new(
            Properties: new RecommendationProperties(
                Category: advisorRecommendation.Properties?.Category,
                Impact: advisorRecommendation.Properties?.Impact,
                ImpactedField: advisorRecommendation.Properties?.ImpactedField,
                ImpactedValue: advisorRecommendation.Properties?.ImpactedValue,
                RecommendationStatus: advisorRecommendation.Properties?.RecommendationStatus,
                CompletionType: advisorRecommendation.Properties?.CompletionType,
                RecommendationDismissReason: advisorRecommendation.Properties?.Reason,
                PostponedUntilDateTime: advisorRecommendation.Properties?.PostponedTime,
                LastRefreshed: advisorRecommendation.Properties?.LastRefreshed,
                LastUpdated: advisorRecommendation.Properties?.LastUpdated,
                CreatedTime: advisorRecommendation.Properties?.CreatedTime,
                RecommendationTypeId: advisorRecommendation.Properties?.RecommendationTypeId,
                ShortDescription: advisorRecommendation.Properties?.ShortDescription is { } shortDescription
                    ? new RecommendationShortDescription(shortDescription.Problem, shortDescription.Solution)
                    : null,
                Metadata: advisorRecommendation.Properties?.Metadata,
                ExtendedProperties: advisorRecommendation.Properties?.ExtendedProperties,
                ResourceMetadata: advisorRecommendation.Properties?.ResourceMetadata is { } resourceMetadata
                    ? new RecommendationResourceMetadata(resourceMetadata.ResourceId)
                    : null,
                Risk: advisorRecommendation.Properties?.Risk,
                Description: advisorRecommendation.Properties?.Description,
                Label: advisorRecommendation.Properties?.Label,
                LearnMoreLink: advisorRecommendation.Properties?.LearnMoreLink,
                PotentialBenefits: advisorRecommendation.Properties?.PotentialBenefits,
                Actions: advisorRecommendation.Properties?.Actions,
                Remediation: advisorRecommendation.Properties?.Remediation,
                ExposedMetadataProperties: advisorRecommendation.Properties?.ExposedMetadataProperties,
                TrackedProperties: advisorRecommendation.Properties?.TrackedProperties,
                Review: advisorRecommendation.Properties?.Review,
                ResourceWorkload: advisorRecommendation.Properties?.ResourceWorkload,
                SourceSystem: advisorRecommendation.Properties?.SourceSystem,
                Notes: advisorRecommendation.Properties?.Notes,
                ServiceGroupId: advisorRecommendation.Properties?.ServiceGroupId,
                Criticality: advisorRecommendation.Properties?.Criticality,
                CriticalityScore: advisorRecommendation.Properties?.CriticalityScore,
                ScoreChangedAt: advisorRecommendation.Properties?.ScoreChangedAt,
                Savings: advisorRecommendation.Properties?.Savings),
            Id: advisorRecommendation.ResourceId,
            Type: advisorRecommendation.ResourceType,
            Name: advisorRecommendation.ResourceName);
    }

    internal static Recommendation ConvertUpdateResponseToAdvisorRecommendationModel(JsonElement item)
    {
        return JsonSerializer.Deserialize(item, AdvisorJsonContext.Default.Recommendation)
            ?? throw new InvalidOperationException("Failed to parse Advisor recommendation update response");
    }

    private static string? GetExtendedPropertyString(
        IReadOnlyDictionary<string, JsonElement>? extendedProperties,
        string propertyName) =>
        extendedProperties is not null &&
        extendedProperties.TryGetValue(propertyName, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

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

    private static RequestFailedException CreateRecommendationUpdateException(Response response)
    {
        string? errorCode = null;

        try
        {
            using var document = JsonDocument.Parse(response.Content.ToStream());
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object &&
                error.TryGetProperty("code", out var code) &&
                code.ValueKind == JsonValueKind.String)
            {
                errorCode = code.GetString();
            }
        }
        catch (JsonException)
        {
            // The status code remains authoritative when the service returns a non-JSON error body.
        }

        var message = errorCode is not null
            ? $"Advisor recommendation update failed with error code '{errorCode}'."
            : $"Advisor recommendation update failed with status code {response.Status}.";

        return new RequestFailedException(
            response.Status,
            message,
            errorCode,
            null);
    }
}
