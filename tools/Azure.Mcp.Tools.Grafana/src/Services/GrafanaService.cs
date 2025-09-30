// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// cSpell:ignore Grafanas

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Grafana.Models;
using Azure.Mcp.Tools.Grafana.Services.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Grafana.Services;

public class GrafanaService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<GrafanaService> logger)
    : BaseAzureResourceService(subscriptionService, tenantService), IGrafanaService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<GrafanaService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IEnumerable<GrafanaWorkspace>> ListWorkspacesAsync(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var workspaces = await ExecuteResourceQueryAsync(
                "Microsoft.Dashboard/grafana",
                resourceGroup: null, // all resource groups
                subscription,
                retryPolicy,
                ConvertToWorkspaceModel,
                cancellationToken: CancellationToken.None);

            return workspaces;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Grafana workspaces: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Converts a JsonElement from Azure Resource Graph query to a Grafana workspace model.
    /// </summary>
    /// <param name="item">The JsonElement containing workspace data</param>
    /// <returns>The Grafana workspace model</returns>
    private static GrafanaWorkspace ConvertToWorkspaceModel(JsonElement item)
    {
        var grafanaWorkspace = ManagedGrafanaData.FromJson(item);
        if (grafanaWorkspace == null)
            throw new InvalidOperationException("Failed to parse Grafana workspace data");

        if (string.IsNullOrEmpty(grafanaWorkspace.ResourceId))
            throw new InvalidOperationException("Resource ID is missing");
        var id = new ResourceIdentifier(grafanaWorkspace.ResourceId);

        return new GrafanaWorkspace(
            Name: grafanaWorkspace.ResourceName,
            ResourceGroupName: id.ResourceGroupName,
            SubscriptionId: id.SubscriptionId,
            Location: grafanaWorkspace.Location,
            Sku: grafanaWorkspace.Sku?.Name,
            ProvisioningState: grafanaWorkspace.Properties?.ProvisioningState,
            Endpoint: grafanaWorkspace.Properties?.Endpoint,
            ZoneRedundancy: grafanaWorkspace.Properties?.ZoneRedundancy,
            PublicNetworkAccess: grafanaWorkspace.Properties?.PublicNetworkAccess,
            GrafanaVersion: grafanaWorkspace.Properties?.GrafanaVersion,
            Identity: grafanaWorkspace.Identity is null ? null : new ManagedIdentityInfo
            {
                SystemAssignedIdentity = new SystemAssignedIdentityInfo
                {
                    Enabled = grafanaWorkspace.Identity != null,
                    TenantId = grafanaWorkspace.Identity?.TenantId?.ToString(),
                    PrincipalId = grafanaWorkspace.Identity?.PrincipalId?.ToString()
                },
                UserAssignedIdentities = grafanaWorkspace.Identity?.UserAssignedIdentities?
                    .Select(identity => new UserAssignedIdentityInfo
                    {
                        ClientId = identity.Value.ClientId?.ToString(),
                        PrincipalId = identity.Value.PrincipalId?.ToString()
                    }).ToArray()
            },
            Tags: grafanaWorkspace.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }
}
