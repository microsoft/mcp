// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.ResourceGroup;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.ApplicationInsights;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public class ApplicationInsightsService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IResourceGroupService resourceGroupService,
    IProfilerDataService profilerDataClient,
    ILogger<ApplicationInsightsService> logger) : BaseAzureService(tenantService), IApplicationInsightsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService;
    private readonly IProfilerDataService _profilerDataClient = profilerDataClient ?? throw new ArgumentNullException(nameof(profilerDataClient));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<JsonNode>> GetProfilerInsightsAsync(string subscription, string? resourceGroup = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);
        List<JsonNode> results = [];

        // Get resource group names ready for processing
        List<string> resourceGroupNames = [];
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            resourceGroupNames.Add(resourceGroup);
        }
        else
        {
            List<ResourceGroupInfo> rgs = await _resourceGroupService.GetResourceGroups(subscription, tenant, retryPolicy);
            resourceGroupNames.AddRange(rgs.Select(rg => rg.Name));
        }

        foreach (string rgName in resourceGroupNames)
        {
            results.AddRange(await GetProfilerInsightsImpAsync(subscription, rgName, tenant, retryPolicy));
            if (results.Count >= 20)
            {
                break;
            }
        }
        return results.Take(20);
    }

    private async Task<IEnumerable<JsonNode>> GetProfilerInsightsImpAsync(string subscription, string resourceGroup, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup);
        List<JsonNode> results = [];

        try
        {
            ResourceGroupResource rgResource = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
                ?? throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
            List<ApplicationInsightsComponentResource> components = await rgResource.GetApplicationInsightsComponents()
                .GetAllAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            IEnumerable<JsonNode> insights = await _profilerDataClient.GetInsightsAsync(resourceIds: components.Select(c => c.Id)).ConfigureAwait(false);
            results.AddRange(insights);

            // Limit to first 20 results to avoid overwhelming output
            return results.Take(20);
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            _logger.LogError(ex, "Error retrieving Application Insights Code Optimization Recommendations");
            throw;
        }
    }
}
