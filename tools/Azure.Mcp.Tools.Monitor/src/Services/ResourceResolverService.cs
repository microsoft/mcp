// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;

namespace Azure.Mcp.Tools.Monitor.Services;

public class ResourceResolverService(ISubscriptionService subscriptionService, ITenantService tenantService)
    : BaseAzureService(tenantService), IResourceResolverService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));

    public async Task<ResourceIdentifier> ResolveResourceIdAsync(
        string subscription,
        string? resourceGroup,
        string? resourceType,
        string resourceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription, resourceName);

        if (ResourceIdentifier.TryParse(resourceName, out ResourceIdentifier? result))
        {
            // If already a valid ResourceIdentifier, return it directly
            return result!;
        }

        // If both resourceGroup and resourceType are provided, build direct path
        if (!string.IsNullOrEmpty(resourceGroup) && !string.IsNullOrEmpty(resourceType))
        {
            return new ResourceIdentifier($"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/{resourceType}/{resourceName}");
        }

        // Need to discover the resource - get subscription resource
        var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy);

        // Get all resources matching the name
        var allMatchingResources = await subscriptionResource.GetGenericResourcesAsync()
            .Where(r => r.Data.Name?.Equals(resourceName, StringComparison.OrdinalIgnoreCase) == true)
            .ToListAsync();

        if (allMatchingResources.Count == 0)
        {
            throw new Exception($"Resource '{resourceName}' not found in subscription '{subscription}'");
        }

        // Apply filtering based on provided parameters
        var filteredResources = allMatchingResources.AsEnumerable();

        // Filter by resource group if provided
        if (!string.IsNullOrEmpty(resourceGroup))
        {
            filteredResources = filteredResources.Where(r =>
                r.Data.Id?.ResourceGroupName?.Equals(resourceGroup, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Filter by resource type if provided
        if (!string.IsNullOrEmpty(resourceType))
        {
            filteredResources = filteredResources.Where(r =>
                r.Data.ResourceType.ToString().Equals(resourceType, StringComparison.OrdinalIgnoreCase));
        }

        var finalResources = filteredResources.ToList();

        if (finalResources.Count == 0)
        {
            var filterInfo = BuildFilterDescription(resourceGroup, resourceType);
            throw new Exception($"No resources named '{resourceName}' found in subscription '{subscription}'{filterInfo}");
        }

        if (finalResources.Count > 1)
        {
            var resourceDetails = finalResources.Select(r =>
                $"- {r.Data.Id} (Resource Group: {r.Data.Id?.ResourceGroupName}, Type: {r.Data.ResourceType})")
                .ToList();

            var filterInfo = BuildFilterDescription(resourceGroup, resourceType);
            throw new Exception($"Multiple resources named '{resourceName}' found in subscription '{subscription}'{filterInfo}. " +
                               $"Please specify both resourceGroup and resourceType parameters to disambiguate. Found resources:\n" +
                               string.Join("\n", resourceDetails));
        }

        return finalResources[0].Data.Id ??
               throw new Exception($"Unable to get resource ID for '{resourceName}'");
    }

    private static string BuildFilterDescription(string? resourceGroup, string? resourceType)
    {
        var filters = new List<string>();

        if (!string.IsNullOrEmpty(resourceGroup))
            filters.Add($"resource group '{resourceGroup}'");

        if (!string.IsNullOrEmpty(resourceType))
            filters.Add($"resource type '{resourceType}'");

        return filters.Count > 0 ? $" with {string.Join(" and ", filters)}" : "";
    }
}
