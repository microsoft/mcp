// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Compute.Models;
using Azure.ResourceManager.Compute.Recommender;
using Azure.ResourceManager.Compute.Recommender.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Compute.Services;

public class ComputePlacementService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<ComputePlacementService> logger)
    : BaseAzureService(tenantService), IComputePlacementService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly ILogger<ComputePlacementService> _logger = logger;

    public async Task<SpotPlacementMetadataInfo> GetSpotPlacementMetadataAsync(
        string location,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(location), location),
            (nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var resourceId = ComputeRecommenderDiagnosticResource.CreateResourceIdentifier(
            subscriptionId, new AzureLocation(location));
        var diagnosticResource = armClient.GetComputeRecommenderDiagnosticResource(resourceId);

        var result = await diagnosticResource.GetAsync(cancellationToken);
        var data = result.Value.Data;

        return new SpotPlacementMetadataInfo(
            Id: data.Id?.ToString(),
            Name: data.Name,
            ResourceType: data.ResourceType.ToString(),
            SupportedResourceTypes: data.ComputeRecommenderDiagnosticSupportedResourceTypes?.ToList() ?? []);
    }

    public async Task<List<PlacementScoreInfo>> GetSpotPlacementScoresAsync(
        string location,
        string subscription,
        string[] desiredLocations,
        string[] desiredSizes,
        int desiredCount = 1,
        bool availabilityZones = true,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(location), location),
            (nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId;

        var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
        var resourceId = ComputeRecommenderDiagnosticResource.CreateResourceIdentifier(
            subscriptionId, new AzureLocation(location));
        var diagnosticResource = armClient.GetComputeRecommenderDiagnosticResource(resourceId);

        var content = new ComputeRecommenderGenerateContent
        {
            DesiredCount = desiredCount,
            AvailabilityZones = availabilityZones
        };

        foreach (var loc in desiredLocations)
        {
            content.DesiredLocations.Add(new AzureLocation(loc));
        }

        foreach (var size in desiredSizes)
        {
            content.DesiredSizes.Add(new ComputeRecommenderResourceSize { Sku = size });
        }

        var result = await diagnosticResource.GenerateAsync(content, cancellationToken);
        var data = result.Value;

        var scores = new List<PlacementScoreInfo>();

        if (data.PlacementScores != null)
        {
            foreach (var score in data.PlacementScores)
            {
                scores.Add(new PlacementScoreInfo(
                    Sku: score.Sku,
                    Region: score.Region?.ToString(),
                    AvailabilityZone: score.AvailabilityZone,
                    Score: score.Score,
                    IsQuotaAvailable: score.IsQuotaAvailable));
            }
        }

        return scores;
    }
}
