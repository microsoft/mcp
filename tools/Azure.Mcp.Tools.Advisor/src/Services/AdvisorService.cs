// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services.Models;
using Azure.Mcp.Tools.Advisor.Validation;
using Azure.ResourceManager;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IAdvisorService
{
    private const string RecommendationUpdateApiVersion = "2026-03-01-preview";
    private const int MaximumRecommendationUpdateRetries = 3;
    private const string RetirementDateProperty =
        "properties.sourceProperties.serviceRetirement.retirementDate";
    private const string TrackingIdsProperty =
        "properties.sourceProperties.serviceRetirement.serviceHealth.trackingIds";

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

    public async Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        RecommendationFilters? filters = null,
        int top = 50,
        string? tenant = null,
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
            tenant: tenant,
            cancellationToken: cancellationToken);
    }

    public async Task<Recommendation> UpdateRecommendationAsync(
        string subscription,
        string recommendationId,
        RecommendationStatus recommendationStatus,
        DateTimeOffset? postponedUntilDateTime = null,
        RecommendationDismissReason? recommendationDismissReason = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(recommendationId), recommendationId));
        RecommendationStateUpdateValidator.Validate(
            recommendationStatus,
            postponedUntilDateTime,
            recommendationDismissReason);

        var subscriptionResource = await AzureService.GetSubscription(
            subscription,
            tenant,
            retryPolicy,
            cancellationToken);
        var subscriptionId = subscriptionResource.Id.SubscriptionId
            ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID.");
        var managementEndpoint = AzureService.CloudConfiguration.ArmEnvironment.Endpoint;
        var accessToken = await GetArmAccessTokenAsync(tenant, cancellationToken);

        var relativePath =
            $"/subscriptions/{Uri.EscapeDataString(subscriptionId)}/providers/Microsoft.Advisor/recommendations/" +
            $"{Uri.EscapeDataString(recommendationId.Trim())}?api-version={RecommendationUpdateApiVersion}";
        var requestUri = new Uri(managementEndpoint, relativePath);
        var properties = new RecommendationStatePatchProperties(
            recommendationStatus,
            recommendationStatus == RecommendationStatus.Postponed ? postponedUntilDateTime : null,
            recommendationStatus == RecommendationStatus.Dismissed ? recommendationDismissReason : null);

        using var client = AzureService.GetClient();
        using var response = await SendRecommendationUpdateAsync(
            client,
            requestUri,
            accessToken.Token,
            properties,
            retryPolicy,
            cancellationToken);

        if (response.IsError)
        {
            throw CreateRecommendationUpdateException(response);
        }

        using var document = JsonDocument.Parse(response.Content.ToStream());

        return ConvertToAdvisorRecommendationModel(document.RootElement);
    }

    private static async Task<Response> SendRecommendationUpdateAsync(
        HttpClient client,
        Uri requestUri,
        string accessToken,
        RecommendationStatePatchProperties properties,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var clientOptions = ConfigureRetryPolicy(
            AddDefaultPolicies(new ArmClientOptions()),
            retryPolicy);
        clientOptions.Retry.MaxRetries = Math.Clamp(
            clientOptions.Retry.MaxRetries,
            0,
            MaximumRecommendationUpdateRetries);
        clientOptions.Transport = new HttpClientTransport(client);

        var pipeline = HttpPipelineBuilder.Build(clientOptions);
        using var request = pipeline.CreateRequest();
        request.Method = RequestMethod.Patch;
        request.Uri.Reset(requestUri);
        request.Headers.Add("Authorization", $"Bearer {accessToken}");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Add("Content-Type", "application/json");
        request.Content = RequestContent.Create(JsonSerializer.SerializeToUtf8Bytes(
            new RecommendationStatePatchRequest(properties),
            AdvisorJsonContext.Default.RecommendationStatePatchRequest));

        return await pipeline.SendRequestAsync(request, cancellationToken);
    }

    public async Task<ResourceQueryResults<RecommendationMetadata>> ListRecommendationMetadataAsync(
        string language,
        RecommendationMetadataFilters? filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(language);

        var tenantResource = await GetTenantResourceAsync(cancellationToken);
        var queryContent = new ResourceQueryContent(
            BuildMetadataListQuery(language, filters));

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        if (result == null || result.Count == 0)
        {
            return new([], false);
        }

        var results = new List<RecommendationMetadata>();
        using var jsonDocument = JsonDocument.Parse(result.Data);
        if (jsonDocument.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new JsonException("Azure Resource Graph returned an invalid recommendation metadata payload.");
        }

        foreach (var item in jsonDocument.RootElement.EnumerateArray())
        {
            results.Add(ConvertToRecommendationMetadataModel(item));
        }

        return new(
            [.. results
            .OrderBy(r => ImpactRank.TryGetValue(r.Impact ?? string.Empty, out var rank) ? rank : int.MaxValue)
            .ThenBy(r => r.DisplayName, StringComparer.OrdinalIgnoreCase)],
            result.ResultTruncated == ResultTruncated.True);
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
        var trackingId = string.IsNullOrWhiteSpace(filters?.TrackingId)
            ? null
            : filters.TrackingId.Trim();
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

        var hasTrackingIdFilter = trackingId is not null;
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

        if (trackingId is not null)
        {
            query += $" | mv-expand trackingId = {TrackingIdsProperty}";
            query += $" | where tostring(trackingId) =~ '{EscapeKqlString(trackingId)}'";
        }

        if (retirementDate is { } date && retirementDateOperator is not null)
        {
            query += $" | where isnotempty(tostring({RetirementDateProperty}))";
            query += $" | where startofday(todatetime({RetirementDateProperty})) " +
                $"{GetKqlComparisonOperator(retirementDateOperator)} " +
                $"datetime({date:yyyy-MM-dd})";
        }

        return query + " | project properties";
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
                "TrackingId and retirement-date filters are only valid for the " +
                $"{RecommendationMetadataFilters.ServiceRetirementSubCategory} subcategory.",
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
    {
        var tenants = await AzureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found.");
        }

        return tenants[0];
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

    public async Task<RecommendationSummary> SummarizeRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        string groupBy,
        RecommendationFilters? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(groupBy);

        var subscriptionResource = await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var allTenants = await AzureService.GetTenants(cancellationToken);
        var tenantResource = allTenants.FirstOrDefault(t => t.Data.TenantId == subscriptionResource.Data.TenantId)
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

        return new(groupBy, totalRecommendations, allGroups);
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
        var advisorRecommendation = Models.RecommendationData.FromJson(item)
            ?? throw new InvalidOperationException("Failed to parse Advisor recommendation data");

        var properties = advisorRecommendation.Properties;
        var resourceId = properties?.ResourceMetadata?.ResourceId ?? "Unknown";

        return new(
            ResourceId: resourceId,
            RecommendationText: properties?.ShortDescription?.Problem ?? "Unknown",
            Category: properties?.Category ?? "Unknown",
            Impact: properties?.Impact,
            ImpactedResourceType: properties?.ImpactedField ?? ParseImpactedResourceType(resourceId),
            RecommendationId: advisorRecommendation.ResourceName,
            Solution: properties?.ShortDescription?.Solution,
            SubCategory: properties?.Control,
            ImpactedResource: properties?.ImpactedValue,
            RecommendationStatus: properties?.RecommendationStatus,
            RecommendationDismissReason: properties?.RecommendationDismissReason,
            PostponedUntilDateTime: properties?.PostponedUntilDateTime,
            LastUpdated: properties?.LastUpdated,
            RecommendationTypeId: properties?.RecommendationTypeId,
            CompletionType: properties?.CompletionType);
    }

    private static RequestFailedException CreateRecommendationUpdateException(Response response)
    {
        string? errorCode = null;
        string? errorMessage = null;

        try
        {
            using var document = JsonDocument.Parse(response.Content.ToStream());
            if (document.RootElement.TryGetProperty("error", out var error) &&
                error.ValueKind == JsonValueKind.Object)
            {
                if (error.TryGetProperty("code", out var code) &&
                    code.ValueKind == JsonValueKind.String)
                {
                    errorCode = code.GetString();
                }

                if (error.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    errorMessage = TruncateErrorMessage(messageElement.GetString());
                }
            }
        }
        catch (JsonException)
        {
            // The status code remains authoritative when the service returns a non-JSON error body.
        }

        var message = (errorCode, errorMessage) switch
        {
            (not null, not null) =>
                $"Advisor recommendation update failed with error code '{errorCode}': {errorMessage}",
            (not null, null) =>
                $"Advisor recommendation update failed with error code '{errorCode}'.",
            _ =>
                $"Advisor recommendation update failed with status code {response.Status}."
        };

        return new RequestFailedException(
            response.Status,
            message,
            errorCode,
            null);
    }

    private static string? TruncateErrorMessage(string? message)
    {
        const int maximumLength = 2048;
        var trimmed = message?.Trim();
        return trimmed?.Length > maximumLength
            ? trimmed[..maximumLength]
            : trimmed;
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
