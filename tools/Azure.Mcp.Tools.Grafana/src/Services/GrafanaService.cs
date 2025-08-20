// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cSpell:ignore Grafanas

using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.Grafana;

namespace Azure.Mcp.Tools.Grafana.Services;

public class GrafanaService(ISubscriptionService _subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IGrafanaService
{
    public async Task<IEnumerable<Models.Workspace.Workspace>> ListWorkspacesAsync(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");
            var workspaces = new List<Models.Workspace.Workspace>();

            await foreach (var workspaceResource in subscriptionResource.GetManagedGrafanasAsync())
            {
                if (string.IsNullOrWhiteSpace(workspaceResource?.Id.ToString())
                    || string.IsNullOrWhiteSpace(workspaceResource.Data.Name))
                {
                    continue;
                }

                var workspace = workspaceResource.Data;
                workspaces.Add(new()
                {
                    Name = workspace.Name,
                    ResourceGroupName = workspaceResource.Id.ResourceGroupName,
                    SubscriptionId = workspaceResource.Id.SubscriptionId,
                    Location = workspace.Location,
                    Sku = workspace.SkuName,
                    ProvisioningState = workspace.Properties.ProvisioningState?.ToString(),
                    Endpoint = workspace.Properties?.Endpoint,
                    ZoneRedundancy = workspace.Properties?.ZoneRedundancy?.ToString(),
                    PublicNetworkAccess = workspace.Properties?.PublicNetworkAccess?.ToString(),
                    GrafanaVersion = workspace.Properties?.GrafanaVersion,
                    Identity = workspace.Identity is null ? null : new ManagedIdentityInfo
                    {
                        SystemAssignedIdentity = new SystemAssignedIdentityInfo
                        {
                            Enabled = workspace.Identity != null,
                            TenantId = workspace.Identity?.TenantId?.ToString(),
                            PrincipalId = workspace.Identity?.PrincipalId?.ToString()
                        },
                        UserAssignedIdentities = workspace.Identity?.UserAssignedIdentities?
                            .Select(identity => new UserAssignedIdentityInfo
                            {
                                ClientId = identity.Value.ClientId?.ToString(),
                                PrincipalId = identity.Value.PrincipalId?.ToString()
                            }).ToArray()
                    },
                    Tags = workspace.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                });
            }

            return workspaces;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to list Grafana workspaces: {ex.Message}", ex);
        }
    }
}
