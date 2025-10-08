// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.ApplicationInsights;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public class ApplicationInsightsService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IResourceGroupService resourceGroupService,
    IProfilerDataService profilerDataClient,
    ILogger<ApplicationInsightsService> logger) : BaseAzureService(tenantService), IApplicationInsightsService
{
    private const int MaxRecommendations = 20;
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService;
    private readonly IProfilerDataService _profilerDataClient = profilerDataClient ?? throw new ArgumentNullException(nameof(profilerDataClient));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<JsonNode>> GetProfilerInsightsAsync(string subscription, string? resourceGroup = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        IEnumerable<JsonNode> results = await GetProfilerInsightsImpAsync(subscription, resourceGroup, tenant, retryPolicy).ConfigureAwait(false);
        return results.Take(MaxRecommendations);
    }

    private async Task<IEnumerable<JsonNode>> GetProfilerInsightsImpAsync(string subscription, string? resourceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        List<JsonNode> results = [];

        try
        {
            List<ApplicationInsightsComponentResource> components = await GetApplicationInsightsComponentsAsync(subscription, resourceGroup, tenant, retryPolicy).ConfigureAwait(false);

            IEnumerable<JsonNode> insights = await _profilerDataClient.GetInsightsAsync(resourceIds: components.Select(c => c.Id)).ConfigureAwait(false);
            results.AddRange(insights);

            // Return all results for this resource group (outer method enforces global max)
            return results;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            _logger.LogError(ex, "Error retrieving Application Insights Code Optimization Recommendations");
            throw;
        }
    }

    private async Task<List<ApplicationInsightsComponentResource>> GetApplicationInsightsComponentsAsync(string subscription, string? resourceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resourceGroup))
        {
            // Query by subscription when resource group is not provided
            return await GetApplicationInsightsComponentsAsync(subscription, tenant, retryPolicy, cancellationToken).ConfigureAwait(false);
        }

        // Otherwise, query by resource group
        ResourceGroupResource rgResource = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
            ?? throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
        return await rgResource.GetApplicationInsightsComponents().GetAllAsync().ToListAsync().ConfigureAwait(false);
    }

    private async Task<List<ApplicationInsightsComponentResource>> GetApplicationInsightsComponentsAsync(string subscription, string? tenant = null, RetryPolicyOptions? retryPolicy = null, CancellationToken cancellationToken = default)
    {
        SubscriptionResource targetSubscription = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy).ConfigureAwait(false);
        return await targetSubscription.GetApplicationInsightsComponentsAsync(cancellationToken).ToListAsync().ConfigureAwait(false);
    }
}
