// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using Azure;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Monitor.Models.HealthModels;
using Azure.ResourceManager.CloudHealth;
using Azure.ResourceManager.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Services;

public class MonitorHealthModelService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<MonitorHealthModelService> logger)
    : BaseAzureService(tenantService), IMonitorHealthModelService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<MonitorHealthModelService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    internal static HealthModelSummary ToSummary(HealthModelData data) =>
        new()
        {
            Id = data.Id?.ToString(),
            Name = data.Name,
            ResourceGroup = data.Id?.ResourceGroupName,
            Location = data.Location.ToString(),
            ProvisioningState = data.HealthModelProvisioningState?.ToString(),
        };

    public async Task<List<HealthModelSummary>> ListHealthModels(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var results = new List<HealthModelSummary>();

        if (string.IsNullOrEmpty(resourceGroup))
        {
            await foreach (var model in subscriptionResource.GetHealthModelsAsync(cancellationToken))
            {
                results.Add(ToSummary(model.Data));
            }
        }
        else
        {
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            await foreach (var model in resourceGroupResource.Value.GetHealthModels().GetAllAsync(cancellationToken: cancellationToken))
            {
                results.Add(ToSummary(model.Data));
            }
        }

        return results;
    }

    internal static HealthModelDetail ToDetail(HealthModelData data, string? healthState) =>
        new()
        {
            Id = data.Id?.ToString(),
            Name = data.Name,
            ResourceGroup = data.Id?.ResourceGroupName,
            Location = data.Location.ToString(),
            ProvisioningState = data.HealthModelProvisioningState?.ToString(),
            HealthState = healthState,
            Identity = ToIdentity(data.Identity),
            Tags = data.Tags,
        };

    internal static HealthModelIdentity? ToIdentity(ManagedServiceIdentity? identity)
    {
        if (identity is null)
        {
            return null;
        }

        return new HealthModelIdentity
        {
            Type = identity.ManagedServiceIdentityType.ToString(),
            PrincipalId = identity.PrincipalId?.ToString(),
            TenantId = identity.TenantId?.ToString(),
            UserAssignedIdentities = identity.UserAssignedIdentities?
                .Keys.Select(id => id.ToString()).ToList(),
        };
    }

    public async Task<HealthModelDetail> GetHealthModel(
        string subscription,
        string resourceGroup,
        string healthModelName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(healthModelName), healthModelName));

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy, cancellationToken);
        var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
        var model = await resourceGroupResource.Value.GetHealthModels().GetAsync(healthModelName, cancellationToken);
        var healthState = await TryGetRootHealthStateAsync(model.Value, healthModelName, cancellationToken);
        return ToDetail(model.Value.Data, healthState);
    }

    private async Task<string?> TryGetRootHealthStateAsync(HealthModelResource model, string rootEntityName, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await model.GetHealthModelEntityAsync(rootEntityName, cancellationToken);
            return entity.Value.Data.Properties?.HealthState?.ToString();
        }
        catch (RequestFailedException ex)
        {
            _logger.LogWarning(ex,
                "Could not resolve root-entity health for health model '{HealthModel}'; returning null healthState. Error: {Message}",
                rootEntityName,
                ex.Message);
            return null;
        }
    }
}
