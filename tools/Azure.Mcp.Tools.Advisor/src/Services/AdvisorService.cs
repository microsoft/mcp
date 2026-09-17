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
        string subscription,
        string? resourceGroup,
        RecommendationFilters? filters = null,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(top, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(top, 100);

        var subscriptionResource = await AzureService.GetSubscription(
            subscription,
            tenant,
            cancellationToken: cancellationToken);
        var metadataTenant = subscriptionResource.Data.TenantId.ToString();

        Dictionary<string, RecommendationMetadata>? metadataByTypeId =
            await ResolveMetadataFilterMatchesAsync(filters, metadataTenant, cancellationToken);

        if (metadataByTypeId is { Count: 0 })
        {
            // Validate the scope so an invalid subscription or resource group fails instead of returning an empty success.
            await ValidateScopeAsync(subscription, resourceGroup, tenant, cancellationToken);
            return new([], false);
        }

        var additionalFilter = RecommendationQueryBuilder.BuildInstancePredicates(
            filters,
            includeStatus: true,
            useRequestedStatus: true,
            includeCategoryAndImpact: true,
            resourceTypeUsesImpactedField: false,
            recommendationTypeIds: metadataByTypeId?.Keys);

        var recommendations = await ExecuteResourceQueryAsync(
            "Microsoft.Advisor/recommendations",
            resourceGroup,
            subscription,
            ConvertToAdvisorRecommendationModel,
            tableName: "advisorresources",
            additionalFilter: additionalFilter,
            limit: top,
            tenant: tenant,
            cancellationToken: cancellationToken);

        if (recommendations.Results.Count == 0)
        {
            return recommendations;
        }

        // Enrich recommendations with matching type-level metadata before returning them.
        metadataByTypeId ??= BuildMetadataLookup(
            await GetRecommendationMetadataByTypeIdsAsync(
                recommendations.Results.Select(r => r.Properties.RecommendationTypeId),
                MetadataJoinLanguage,
                metadataTenant,
                cancellationToken));

        return new(
            JoinWithMetadata(recommendations.Results, metadataByTypeId),
            recommendations.AreResultsTruncated);
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

    private async Task<SubscriptionResource> ValidateScopeAsync(
        string subscription,
        string? resourceGroup,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(resourceGroup))
        {
            var rgExists = await subscriptionResource.GetResourceGroups().ExistsAsync(resourceGroup, cancellationToken);
            if (!rgExists.Value)
            {
                throw new KeyNotFoundException(
                    $"Resource group '{resourceGroup}' does not exist in subscription '{subscriptionResource.Data.SubscriptionId}'");
            }
        }

        return subscriptionResource;
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

    internal static List<Recommendation> JoinWithMetadata(
        IEnumerable<Recommendation> recommendations,
        IReadOnlyDictionary<string, RecommendationMetadata> metadataByTypeId)
    {
        var joined = new List<Recommendation>();

        foreach (var recommendation in recommendations)
        {
            if (string.IsNullOrWhiteSpace(recommendation.Properties.RecommendationTypeId) ||
                !metadataByTypeId.TryGetValue(recommendation.Properties.RecommendationTypeId, out var metadata))
            {
                joined.Add(recommendation);
                continue;
            }

            joined.Add(recommendation with
            {
                Properties = recommendation.Properties with
                {
                    Category = metadata.Category ?? recommendation.Properties.Category,
                    Impact = metadata.Impact ?? recommendation.Properties.Impact,
                    ShortDescription = recommendation.Properties.ShortDescription ??
                        (metadata.DisplayName is null
                            ? null
                            : new RecommendationShortDescription(metadata.DisplayName, metadata.DisplayName)),
                    Description = metadata.DetailedDescription ?? recommendation.Properties.Description,
                    // Recommendations from an external source system carry a per-instance label that differs from the catalog, so prefer it.
                    Label = string.IsNullOrWhiteSpace(recommendation.Properties.SourceSystem)
                        ? metadata.Label ?? recommendation.Properties.Label
                        : recommendation.Properties.Label ?? metadata.Label,
                    LearnMoreLink = metadata.LearnMoreLink ?? recommendation.Properties.LearnMoreLink,
                    PotentialBenefits = metadata.PotentialBenefits ?? recommendation.Properties.PotentialBenefits,
                    ExtendedProperties = AddMetadataSubCategory(
                        AddMetadataRetirementProperties(
                            recommendation.Properties.ExtendedProperties,
                            metadata.ServiceRetirement),
                        metadata.SubCategory),
                },
            });
        }

        return joined;
    }

    private async Task<List<RecommendationMetadata>> GetRecommendationMetadataByTypeIdsAsync(
        IEnumerable<string?> recommendationTypeIds,
        string language,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var distinctIds = recommendationTypeIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinctIds.Count == 0)
        {
            return [];
        }

        return await ExecuteMetadataQueryAsync(
            BuildMetadataByTypeIdsQuery(distinctIds, language),
            tenant,
            cancellationToken);
    }

    internal static string BuildMetadataByTypeIdsQuery(
        IReadOnlyCollection<string> recommendationTypeIds,
        string language) =>
        "advisorresources " +
        "| where type =~ 'microsoft.advisor/metadata' " +
        $"| where tostring(properties.language) =~ '{EscapeKqlString(language.Trim())}' " +
        $"| where tostring(properties.recommendationTypeId) in~ ({FormatKqlStringList(recommendationTypeIds)}) " +
        "| project properties";

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

    private async Task<List<RecommendationMetadata>> ExecuteMetadataQueryAsync(
        string query,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var tenantResource = await GetTenantResourceAsync(tenant, cancellationToken);

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(
            new ResourceQueryContent(query),
            cancellationToken);

        if (result == null || result.Count == 0)
        {
            return [];
        }

        return ParseMetadata(result.Data);
    }

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
                Notes: advisorRecommendation.Properties?.Notes),
            Id: advisorRecommendation.ResourceId,
            Type: advisorRecommendation.ResourceType,
            Name: advisorRecommendation.ResourceName);
    }

    internal static Recommendation ConvertUpdateResponseToAdvisorRecommendationModel(JsonElement item)
    {
        return JsonSerializer.Deserialize(item, AdvisorJsonContext.Default.Recommendation)
            ?? throw new InvalidOperationException("Failed to parse Advisor recommendation update response");
    }

    private static IReadOnlyDictionary<string, JsonElement>? AddMetadataSubCategory(
        IReadOnlyDictionary<string, JsonElement>? properties,
        string? subCategory)
    {
        if (string.IsNullOrWhiteSpace(subCategory))
        {
            return properties;
        }

        var result = properties is null
            ? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, JsonElement>(properties, StringComparer.OrdinalIgnoreCase);
        result["recommendationSubCategory"] = JsonSerializer.SerializeToElement(
            subCategory,
            Commands.AdvisorJsonContext.Default.String);
        return result;
    }

    private static IReadOnlyDictionary<string, JsonElement>? AddMetadataRetirementProperties(
        IReadOnlyDictionary<string, JsonElement>? properties,
        RecommendationServiceRetirement? serviceRetirement)
    {
        if (serviceRetirement is null)
        {
            return properties;
        }

        var result = properties is null
            ? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, JsonElement>(properties, StringComparer.OrdinalIgnoreCase);

        AddStringProperty(result, "retirementDate", serviceRetirement.RetirementDate);
        AddStringProperty(result, "retirementFeatureName", serviceRetirement.RetirementFeatureName);
        return result;
    }

    private static void AddStringProperty(
        IDictionary<string, JsonElement> properties,
        string name,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            properties[name] = JsonSerializer.SerializeToElement(
                value,
                Commands.AdvisorJsonContext.Default.String);
        }
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
