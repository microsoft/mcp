// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Containers.ContainerRegistry;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.ContainerRegistry;

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

    public async Task<Dictionary<string, List<string>>> ListRegistryRepositories(
        string subscription,
        string? resourceGroup = null,
        string? registry = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);
        var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        async Task AddRepositoriesForRegistryAsync(ContainerRegistryResource reg)
        {
            var data = reg.Data;
            if (data?.LoginServer is null || string.IsNullOrWhiteSpace(data.Name))
            {
                return;
            }

            // Build data-plane client for this login server
            var credential = await GetCredential(tenant);
            var options = ConfigureRetryPolicy(AddDefaultPolicies(new ContainerRegistryClientOptions()), retryPolicy);
            var acrEndpoint = new Uri($"https://{data.LoginServer}");
            var client = new ContainerRegistryClient(acrEndpoint, credential, options);

            var repoNames = new List<string>();
            try
            {
                await foreach (var repo in client.GetRepositoryNamesAsync())
                {
                    if (!string.IsNullOrWhiteSpace(repo))
                    {
                        repoNames.Add(repo);
                    }
                }
            }
            catch (RequestFailedException)
            {
                // If we cannot enumerate repositories (e.g., permissions), return empty list for that registry
            }

            result[data.Name] = repoNames;
        }

        if (!string.IsNullOrWhiteSpace(registry))
        {
            // Fetch a single registry by name, optionally within the provided resource group
            if (!string.IsNullOrWhiteSpace(resourceGroup))
            {
                var rg = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
                var regRes = await rg.Value.GetContainerRegistries().GetAsync(registry);
                await AddRepositoriesForRegistryAsync(regRes.Value);
            }
            else
            {
                // enumerate across subscription to find the registry name
                await foreach (var regRes in subscriptionResource.GetContainerRegistriesAsync())
                {
                    if (string.Equals(regRes.Data?.Name, registry, StringComparison.OrdinalIgnoreCase))
                    {
                        await AddRepositoriesForRegistryAsync(regRes);
                        break;
                    }
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(resourceGroup))
        {
            var rg = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            await foreach (var regRes in rg.Value.GetContainerRegistries().GetAllAsync())
            {
                await AddRepositoriesForRegistryAsync(regRes);
            }
        }
        else
        {
            await foreach (var regRes in subscriptionResource.GetContainerRegistriesAsync())
            {
                await AddRepositoriesForRegistryAsync(regRes);
            }
        }

        return result;
    }
}
