using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;

namespace Azure.Mcp.Tools.Playwright.Services;

using Azure.Mcp.Tools.Playwright.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Playwright;
using Azure.ResourceManager.Resources;

// Minimal scaffold service - currently returns empty results or simple dummies
public class PlaywrightService(ISubscriptionService subscriptionService) : BaseAzureService, IPlaywrightService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService;

    public async Task<List<PlaywrightWorkspace>> GetPlaywrightWorkspacesAsync(string subscription, string? resourceGroup = null, string? workspaceName = null, string? tenant = null)
    {
        ValidateRequiredParameters(subscription);
        var subscriptionId = (await _subscriptionService.GetSubscription(subscription, tenant)).Data.SubscriptionId;

        var credential = await GetCredential(tenant);

        var client = new ArmClient(credential);
        if (!string.IsNullOrEmpty(workspaceName))
        {
            var resourceId = PlaywrightWorkspaceResource.CreateResourceIdentifier(subscriptionId, resourceGroup, workspaceName);
            var response = await client.GetPlaywrightWorkspaceResource(resourceId).GetAsync();

            if (response == null)
            {
                throw new Exception($"Failed to retrieve Azure Load Testing resources: {response}");
            }
            return new List<PlaywrightWorkspace>
            {
                new PlaywrightWorkspace
                {
                    Id = response.Value.Data.Id!,
                    Name = response.Value.Data.Name,
                    Location = response.Value.Data.Location,
                    DataPlaneUri = response.Value.Data.Properties.DataplaneUri.ToString(),
                    ProvisioningState = response.Value.Data.Properties.ProvisioningState?.ToString(),
                }
            };
        }
        else
        {
            var rgResource = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
            var response = client.GetResourceGroupResource(rgResource).GetPlaywrightWorkspaces().ToList();

            if (response == null || response.Count == 0)
            {
                throw new Exception($"Failed to retrieve Azure Load Testing resources: {response}");
            }
            var playwrightWorkspaceResources = new List<PlaywrightWorkspace>();
            foreach (var resource in response)
            {
                playwrightWorkspaceResources.Add(new PlaywrightWorkspace
                {
                    Id = resource.Data.Id!,
                    Name = resource.Data.Name,
                    Location = resource.Data.Location,
                    DataPlaneUri = resource.Data.Properties.DataplaneUri.ToString(),
                    ProvisioningState = resource.Data.Properties.ProvisioningState?.ToString(),
                });
            }
            return playwrightWorkspaceResources;
        }
    }

    public async Task<PlaywrightWorkspace?> GetPlaywrightWorkspaceAsync(string subscription, string resourceGroup, string workspaceName, string? tenant = null)
    {
        var credential = await GetCredential(tenant);
        var subscriptionId = (await _subscriptionService.GetSubscription(subscription, tenant)).Data.SubscriptionId;
        var client = new ArmClient(credential);
        var resourceId = PlaywrightWorkspaceResource.CreateResourceIdentifier(subscriptionId, resourceGroup, workspaceName);
        var response = await client.GetPlaywrightWorkspaceResource(resourceId).GetAsync();
        if (response == null)
        {
            return null;
        }
        return new PlaywrightWorkspace
        {
            Id = response.Value.Data.Id!,
            Name = response.Value.Data.Name,
            Location = response.Value.Data.Location,
            DataPlaneUri = response.Value.Data.Properties.DataplaneUri.ToString(),
            ProvisioningState = response.Value.Data.Properties.ProvisioningState?.ToString(),
        };
    }

    public async Task<PlaywrightWorkspace> CreateOrUpdatePlaywrightWorkspaceAsync(string subscription, string resourceGroup, PlaywrightWorkspaceCreateOrUpdateRequest request, string? tenant = null)
    {
        ValidateRequiredParameters(subscription, resourceGroup);
        var subscriptionId = (await _subscriptionService.GetSubscription(subscription, tenant)).Data.SubscriptionId;
        var credential = await GetCredential(tenant);
        var client = new ArmClient(credential);
        var rgResource = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroup);
        var rg = client.GetResourceGroupResource(rgResource);
        var location = rg.Get().Value.Data.Location;
        var playwrightWorkspaces = await rg.GetPlaywrightWorkspaces().CreateOrUpdateAsync(WaitUntil.Completed, request?.Name, new PlaywrightWorkspaceData(location));

        if (playwrightWorkspaces == null)
        {
            throw new Exception($"Failed to create or update Playwright workspace: {request?.Name}");
        }
        return new PlaywrightWorkspace
        {
            Id = playwrightWorkspaces.Value.Data.Id!,
            Name = playwrightWorkspaces.Value.Data.Name,
            Location = playwrightWorkspaces.Value.Data.Location,
            DataPlaneUri = playwrightWorkspaces.Value.Data.Properties.DataplaneUri.ToString(),
            ProvisioningState = playwrightWorkspaces.Value.Data.Properties.ProvisioningState?.ToString(),
        };

    }

    public Task<PlaywrightWorkspace> UpdatePlaywrightWorkspaceAsync(string subscription, string resourceGroup, string workspaceName, PlaywrightWorkspaceUpdateRequest request, string? tenant = null)
    {
        throw new NotImplementedException("Update for PlaywrightWorkspaces is not yet implemented. Please implement using Azure.ResourceManager.PlaywrightTesting SDK.");
    }

    public Task DeletePlaywrightWorkspaceAsync(string subscription, string resourceGroup, string workspaceName, string? tenant = null)
    {
        throw new NotImplementedException("Delete for PlaywrightWorkspaces is not yet implemented. Please implement using Azure.ResourceManager.PlaywrightTesting SDK.");
    }

    public Task<List<PlaywrightQuota>> GetSubscriptionPlaywrightQuotasAsync(string subscription, string location, string? tenant = null)
    {
        // Placeholder
        return Task.FromResult(new List<PlaywrightQuota>());
    }

    public Task<PlaywrightQuota?> GetSubscriptionPlaywrightQuotaAsync(string subscription, string location, string quotaName, string? tenant = null)
    {
        return Task.FromResult<PlaywrightQuota?>(null);
    }

    public Task<List<PlaywrightWorkspaceQuota>> GetPlaywrightWorkspaceQuotasAsync(string subscription, string resourceGroup, string workspaceName, string? tenant = null)
    {
        return Task.FromResult(new List<PlaywrightWorkspaceQuota>());
    }

    public Task<PlaywrightWorkspaceQuota?> GetPlaywrightWorkspaceQuotaAsync(string subscription, string resourceGroup, string workspaceName, string quotaName, string? tenant = null)
    {
        return Task.FromResult<PlaywrightWorkspaceQuota?>(null);
    }
}
