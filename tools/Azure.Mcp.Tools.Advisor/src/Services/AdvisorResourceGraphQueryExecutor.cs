// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Advisor.Services;

internal sealed class AdvisorResourceGraphQueryExecutor(IAzureService azureService)
{
    private readonly IAzureService _azureService = azureService;

    internal async Task<(RecommendationQueryScope Scope, TenantResource TenantResource)> ResolveSubscriptionScopeAsync(
        string subscription,
        string? resourceGroup,
        string? tenant,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        var subscriptionResource = await _azureService.GetSubscription(
            subscription,
            tenant,
            cancellationToken: cancellationToken);
        var subscriptionId = subscriptionResource.Data.SubscriptionId
            ?? throw new InvalidOperationException("The resolved Azure subscription does not have a subscription ID.");
        var normalizedResourceGroup = string.IsNullOrWhiteSpace(resourceGroup)
            ? null
            : resourceGroup.Trim();

        if (normalizedResourceGroup is not null)
        {
            var exists = await subscriptionResource
                .GetResourceGroups()
                .ExistsAsync(normalizedResourceGroup, cancellationToken);
            if (!exists.Value)
            {
                throw new KeyNotFoundException(
                    $"Resource group '{normalizedResourceGroup}' does not exist in subscription '{subscriptionId}'.");
            }
        }

        var tenantResource = await ResolveTenantResourceAsync(
            subscriptionResource.Data.TenantId,
            cancellationToken);
        return (
            RecommendationQueryScope.ForSubscription(subscriptionId, normalizedResourceGroup),
            tenantResource);
    }

    internal async Task<(RecommendationQueryScope Scope, TenantResource TenantResource)> ResolveServiceGroupScopeAsync(
        string serviceGroup,
        string? tenant,
        CancellationToken cancellationToken)
    {
        var scope = RecommendationQueryScope.ForServiceGroup(serviceGroup);
        var tenantResource = await ResolveTenantResourceAsync(tenant, cancellationToken);
        return (scope, tenantResource);
    }

    internal async Task<TenantResource> ResolveTenantResourceAsync(
        string? tenant,
        CancellationToken cancellationToken)
    {
        var tenants = await _azureService.GetTenants(cancellationToken);
        if (tenants.Count == 0)
        {
            throw new InvalidOperationException("No accessible Azure tenants were found.");
        }

        if (string.IsNullOrWhiteSpace(tenant))
        {
            return tenants[0];
        }

        var resolvedTenantId = await _azureService.ResolveTenantIdAsync(tenant, cancellationToken)
            ?? throw new InvalidOperationException($"Could not resolve tenant '{tenant}'.");
        if (!Guid.TryParse(resolvedTenantId, out var tenantId))
        {
            throw new InvalidOperationException($"Resolved tenant '{tenant}' does not have a valid tenant ID.");
        }

        return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
            ?? throw new InvalidOperationException($"No accessible tenant found for tenant '{tenant}'.");
    }

    internal async Task<TenantResource> ResolveTenantResourceAsync(
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        if (tenantId is null)
        {
            throw new InvalidOperationException("The resolved Azure subscription does not have a tenant ID.");
        }

        var tenants = await _azureService.GetTenants(cancellationToken);
        return tenants.FirstOrDefault(candidate => candidate.Data.TenantId == tenantId)
            ?? throw new InvalidOperationException(
                $"No accessible tenant found for tenant ID '{tenantId}'.");
    }

    /// <summary>Runs <paramref name="query"/> at tenant scope; the query's own predicates select the subscription or service group.</summary>
    internal static async Task<ResourceQueryResult> ExecuteAsync(
        TenantResource tenantResource,
        string query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tenantResource);
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        var response = await tenantResource.GetResourcesAsync(new ResourceQueryContent(query), cancellationToken);
        return response.Value;
    }
}
