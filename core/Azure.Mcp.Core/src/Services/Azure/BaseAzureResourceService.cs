// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Base class for Azure services that need to query Azure Resource Graph for resource management operations.
/// Provides common methods for executing resource queries against Azure Resource Manager resources.
/// </summary>
public abstract class BaseAzureResourceService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILoggerFactory? loggerFactory = null) : BaseAzureService(tenantService, loggerFactory)
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ITenantService _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));

    /// <summary>
    /// Gets the tenant resource for the specified subscription.
    /// </summary>
    /// <param name="tenantId">The tenant ID from the subscription</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The tenant resource associated with the subscription</returns>
    private async Task<TenantResource> GetTenantResourceAsync(Guid? tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == null || tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));
        }

        // Get all tenants and find the matching one (GetTenants already has caching)
        var allTenants = await _tenantService.GetTenants();
        var tenantResource = allTenants.FirstOrDefault(t => t.Data.TenantId == tenantId.Value);

        if (tenantResource == null)
        {
            throw new InvalidOperationException($"No accessible tenant found for tenant ID '{tenantId}'");
        }

        return tenantResource;
    }

    /// <summary>
    /// Executes a Resource Graph query and returns a list of resources of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert each resource to</typeparam>
    /// <param name="resourceType">The Azure resource type to query for (e.g., "Microsoft.Sql/servers/databases")</param>
    /// <param name="resourceGroup">The resource group name to filter by (null to query all resource groups)</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <param name="converter">Function to convert JsonElement to the target type</param>
    /// <param name="additionalFilter">Optional additional KQL filter conditions</param>
    /// <param name="limit">Maximum number of results to return (default: 50)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of resources converted to the specified type</returns>
    protected async Task<List<T>> ExecuteResourceQueryAsync<T>(
        string resourceType,
        string? resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        Func<JsonElement, T> converter,
        string? additionalFilter = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(resourceType, subscription);
        ArgumentNullException.ThrowIfNull(converter);

        var results = new List<T>();

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);
        var tenantResource = await GetTenantResourceAsync(subscriptionResource.Data.TenantId, cancellationToken);

        var queryFilter = $"Resources | where type =~ '{EscapeKqlString(resourceType)}'";
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            queryFilter += $" and resourceGroup =~ '{EscapeKqlString(resourceGroup)}'";
        }
        if (!string.IsNullOrEmpty(additionalFilter))
        {
            queryFilter += $" and {additionalFilter}";
        }
        queryFilter += $" | limit {limit}";

        var queryContent = new ResourceQueryContent(queryFilter)
        {
            Subscriptions = { subscriptionResource.Data.SubscriptionId }
        };

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        if (result != null && result.Count > 0)
        {
            using var jsonDocument = JsonDocument.Parse(result.Data);
            var dataArray = jsonDocument.RootElement;
            if (dataArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in dataArray.EnumerateArray())
                {
                    results.Add(converter(item));
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Executes a Resource Graph query and returns a single resource of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the resource to</typeparam>
    /// <param name="resourceType">The Azure resource type to query for (e.g., "Microsoft.Sql/servers/databases")</param>
    /// <param name="resourceGroup">The resource group name to filter by (null to query all resource groups)</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration</param>
    /// <param name="converter">Function to convert JsonElement to the target type</param>
    /// <param name="additionalFilter">Optional additional KQL filter conditions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Single resource converted to the specified type, or null if not found</returns>
    protected async Task<T?> ExecuteSingleResourceQueryAsync<T>(
        string resourceType,
        string? resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        Func<JsonElement, T> converter,
        string? additionalFilter = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ValidateRequiredParameters(resourceType, subscription);
        ArgumentNullException.ThrowIfNull(converter);

        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);
        var tenantResource = await GetTenantResourceAsync(subscriptionResource.Data.TenantId, cancellationToken);

        var queryFilter = $"Resources | where type =~ '{EscapeKqlString(resourceType)}'";
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            queryFilter += $" and resourceGroup =~ '{EscapeKqlString(resourceGroup)}'";
        }
        if (!string.IsNullOrEmpty(additionalFilter))
        {
            queryFilter += $" and {additionalFilter}";
        }
        queryFilter += " | limit 1";

        var queryContent = new ResourceQueryContent(queryFilter)
        {
            Subscriptions = { subscriptionResource.Data.SubscriptionId }
        };

        ResourceQueryResult result = await tenantResource.GetResourcesAsync(queryContent, cancellationToken);
        if (result != null && result.Count > 0)
        {
            using var jsonDocument = JsonDocument.Parse(result.Data);
            var dataArray = jsonDocument.RootElement;
            var item = dataArray.ValueKind == JsonValueKind.Array && dataArray.GetArrayLength() > 0
                ? dataArray[0]
                : default;
            if (item.ValueKind == JsonValueKind.Object)
            {
                return converter(item);
            }
        }
        return null;
    }
}
