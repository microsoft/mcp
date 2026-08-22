// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Advisor.Services;

public interface IAdvisorService
{
    Task<ResourceQueryResults<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        RecommendationFilters? filters = null,
        int top = 50,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<RecommendationSummary> SummarizeRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        string groupBy,
        RecommendationFilters? filters = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<Recommendation> UpdateRecommendationAsync(
        string subscription,
        string recommendationId,
        RecommendationStatus recommendationStatus,
        DateTimeOffset? postponedUntilDateTime = null,
        RecommendationDismissReason? recommendationDismissReason = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<ResourceQueryResults<RecommendationMetadata>> ListRecommendationMetadataAsync(
        string language,
        RecommendationMetadataFilters? filters,
        CancellationToken cancellationToken = default);

    Task<RecommendationMetadata?> GetRecommendationMetadataAsync(
        string recommendationTypeId,
        string language,
        CancellationToken cancellationToken = default);
}
