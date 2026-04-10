// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Advisor.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Advisor.Services;

public class AdvisorService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<AdvisorService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IAdvisorService
{
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
