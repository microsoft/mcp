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

public class AdvisorService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<AdvisorService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IAdvisorService
{
    private readonly ILogger<AdvisorService> _logger = logger;

    public async Task<List<Recommendation>> ListRecommendationsAsync(
        string subscription,
        string? resourceGroup,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Advisor recommendations. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}",
                subscription, resourceGroup);
            throw;
        }
    }

    private static Recommendation ConvertToAdvisorRecommendationModel(JsonElement item)
    {
        Models.RecommendationData? advisorRecommendation = Models.RecommendationData.FromJson(item);
        if (advisorRecommendation == null)
        {
            throw new InvalidOperationException("Failed to parse Advisor recommendation data");
        }

        return new Recommendation(
                ResourceId: advisorRecommendation?.Properties?.ResourceMetadata?.ResourceId ?? "Unknown",
                RecommendationText: advisorRecommendation?.Properties?.ShortDescription?.Problem ?? "Unknown",
                Category: advisorRecommendation?.Properties?.Category ?? "Unknown"
            );
    }
}
