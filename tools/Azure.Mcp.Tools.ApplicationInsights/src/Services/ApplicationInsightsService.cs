// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ApplicationInsights.Models;
using Azure.ResourceManager.ApplicationInsights;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public class ApplicationInsightsService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IResourceGroupService resourceGroupService) : BaseAzureService(tenantService), IApplicationInsightsService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService;

    public async Task<List<ApplicationInsightsInfo>> ListApplicationInsights(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);
        var results = new List<ApplicationInsightsInfo>();

        try
        {
            if (!string.IsNullOrEmpty(resourceGroup))
            {
                var rgResource = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
                    ?? throw new Exception($"Resource group {resourceGroup} not found in subscription {subscription}");
                var components = await rgResource.GetApplicationInsightsComponents()
                    .GetAllAsync()
                    .ToListAsync()
                    .ConfigureAwait(false);

                results.AddRange(components.Select(c => new ApplicationInsightsInfo
                {
                    Name = c.Data.Name ?? string.Empty,
                    Id = c.Data.Id?.ToString() ?? string.Empty,
                    Location = c.Data.Location.ToString(),
                    AppId = c.Data.AppId,
                    InstrumentationKey = c.Data.InstrumentationKey
                }));
            }
            else
            {
                var rgs = await _resourceGroupService.GetResourceGroups(subscription, tenant, retryPolicy);
                foreach (var rg in rgs)
                {
                    try
                    {
                        var rgResource = await _resourceGroupService.GetResourceGroupResource(subscription, rg.Name, tenant, retryPolicy);
                        if (rgResource == null) continue;
                        var components = await rgResource.GetApplicationInsightsComponents()
                            .GetAllAsync()
                            .ToListAsync()
                            .ConfigureAwait(false);
                        results.AddRange(components.Select(c => new ApplicationInsightsInfo
                        {
                            Name = c.Data.Name ?? string.Empty,
                            Id = c.Data.Id?.ToString() ?? string.Empty,
                            Location = c.Data.Location.ToString(),
                            AppId = c.Data.AppId,
                            InstrumentationKey = c.Data.InstrumentationKey
                        }));
                    }
                    catch
                    {
                        // Ignore RG level failures
                    }
                }
            }

            return results
                .Where(ai => !string.IsNullOrEmpty(ai.Name))
                .OrderBy(ai => ai.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new Exception($"Error retrieving Application Insights components: {ex.Message}", ex);
        }
    }
}
