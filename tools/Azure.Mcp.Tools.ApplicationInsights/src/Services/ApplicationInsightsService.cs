// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager.ApplicationInsights;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public class ApplicationInsightsService(IAzureService azureService, IProfilerDataService profilerDataClient)
    : BaseAzureService(azureService), IApplicationInsightsService
{
    private const int MaxRecommendations = 20;
    private readonly IProfilerDataService _profilerDataClient = profilerDataClient ?? throw new ArgumentNullException(nameof(profilerDataClient));

    public async Task<IEnumerable<JsonNode>> GetProfilerInsightsAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));
        IEnumerable<JsonNode> results = await GetProfilerInsightsImpAsync(subscription, resourceGroup, tenant, retryPolicy, cancellationToken).ConfigureAwait(false);
        return results.Take(MaxRecommendations);
    }

    private async Task<IEnumerable<JsonNode>> GetProfilerInsightsImpAsync(
        string subscription,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        List<JsonNode> results = [];
        var components = await GetApplicationInsightsComponentsAsync(subscription, resourceGroup, tenant, retryPolicy, cancellationToken).ConfigureAwait(false);

        var insights = await _profilerDataClient.GetInsightsAsync(resourceIds: components.Select(c => c.Id), cancellationToken: cancellationToken).ConfigureAwait(false);
        results.AddRange(insights);

        // Return all results for this resource group (outer method enforces global max)
        return results;
    }

    private async Task<List<ApplicationInsightsComponentResource>> GetApplicationInsightsComponentsAsync(
        string subscription,
        string? resourceGroup,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(resourceGroup))
        {
            // Query by subscription when resource group is not provided
            return await GetApplicationInsightsComponentsAsync(subscription, tenant, retryPolicy, cancellationToken).ConfigureAwait(false);
        }

        // Otherwise, query by resource group
        var rgResource = await AzureService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy, cancellationToken)
            ?? throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
        return await rgResource.GetApplicationInsightsComponents().GetAllAsync(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<List<ApplicationInsightsComponentResource>> GetApplicationInsightsComponentsAsync(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var targetSubscription = await AzureService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken).ConfigureAwait(false);
        return await targetSubscription.GetApplicationInsightsComponentsAsync(cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
