// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.ContainerRegistry;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;

namespace Azure.Mcp.Tools.Acr.Services;

public sealed class AcrService(ISubscriptionService subscriptionService, ITenantService tenantService) : BaseAzureService(tenantService), IAcrService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<List<Models.AcrRegistryInfo>> ListRegistries(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var registries = new List<Models.AcrRegistryInfo>();

        // Select enumeration source based on optional resource group
        if (!string.IsNullOrWhiteSpace(resourceGroup))
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            await foreach (var registry in resourceGroupResource.Value.GetContainerRegistries().GetAllAsync())
            {
                AddProjectionIfValid(registries, registry);
            }
        }
        else
        {
            await foreach (var registry in subscriptionResource.GetContainerRegistriesAsync())
            {
                AddProjectionIfValid(registries, registry);
            }
        }

        return registries;
    }

    private static void AddProjectionIfValid(List<Models.AcrRegistryInfo> list, ContainerRegistryResource? registry)
    {
        var data = registry?.Data;
        if (data?.Name is null)
        {
            return;
        }

        list.Add(new Models.AcrRegistryInfo(
            data.Name,
            data.Location,
            data.LoginServer,
            data.Sku?.Name.ToString(),
            data.Sku?.Tier?.ToString()));
    }
}
