// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureManagedLustre.Models;
using Azure.ResourceManager.StorageCache;
using Azure.ResourceManager.StorageCache.Models;

namespace Azure.Mcp.Tools.AzureManagedLustre.Services;

public sealed class AzureManagedLustreService(ISubscriptionService subscriptionService, IResourceGroupService resourceGroupService, ITenantService tenantService) : BaseAzureService(tenantService), IAzureManagedLustreService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly IResourceGroupService _resourceGroupService = resourceGroupService;

    public async Task<List<LustreFileSystem>> ListFileSystemsAsync(string subscription, string? resourceGroup = null, string? tenant = null, RetryPolicyOptions? retryPolicy = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        var results = new List<LustreFileSystem>();

        try
        {
            if (!string.IsNullOrWhiteSpace(resourceGroup))
            {
                var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy) ?? throw new Exception($"Resource group '{resourceGroup}' not found");
                foreach (var fs in rg.GetAmlFileSystems())
                {
                    results.Add(Map(fs));
                }
                return results;
            }
            else
            {
                var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");
                await foreach (var fs in sub.GetAmlFileSystemsAsync())
                {
                    results.Add(Map(fs));
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing AMLFS file systems in subscription '{subscription}': {ex.Message}", ex);
        }

        return results;
    }

    private static LustreFileSystem Map(AmlFileSystemResource fs)
    {
        var data = fs.Data;
        return new LustreFileSystem(
            data.Name,
            fs.Id.ToString(),
            fs.Id.ResourceGroupName,
            fs.Id.SubscriptionId,
            data.Location,
            data.ProvisioningState?.ToString(),
            data.Health?.ToString(),
            data.ClientInfo?.MgsAddress,
            data.SkuName,
            data.StorageCapacityTiB.HasValue ? Convert.ToInt64(Math.Round(data.StorageCapacityTiB.Value)) : null,
            data.Hsm?.Settings?.Container,
            data.MaintenanceWindow?.DayOfWeek?.ToString(),
            data.MaintenanceWindow?.TimeOfDayUTC?.ToString()
        );
    }

    private static List<AzureManagedLustreSkuCapability> MapCapabilities(IEnumerable<StorageCacheSkuCapability>? caps)
    {
        var list = new List<AzureManagedLustreSkuCapability>();
        if (caps is null)
            return list;
        foreach (var cap in caps)
        {
            var name = cap?.Name;
            if (string.IsNullOrWhiteSpace(name))
                continue;
            list.Add(new AzureManagedLustreSkuCapability(name!, cap?.Value ?? string.Empty));
        }
        return list;
    }

    public async Task<int> GetRequiredAmlFSSubnetsSize(string subscription,
    string sku, int size,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null
        )
    {
        var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");
        var fileSystemSizeContent = new RequiredAmlFileSystemSubnetsSizeContent
        {
            SkuName = sku,
            StorageCapacityTiB = size
        };

        try
        {
            var sdkResult = await sub.GetRequiredAmlFSSubnetsSizeAsync(fileSystemSizeContent);
            var numberOfRequiredIPs = sdkResult.Value.FilesystemSubnetSize ?? throw new Exception($"Failed to retrieve the number of IPs");
            return numberOfRequiredIPs;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving required subnet size: {ex.Message}", ex);
        }
    }

    public async Task<List<AzureManagedLustreSkuInfo>> SkuGetInfoAsync(
        string subscription,
        string? tenant = null,
        string? location = null,
        RetryPolicyOptions? retryPolicy = null
        )
    {
        ValidateRequiredParameters(subscription);

        var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");

        try
        {
            var results = new List<AzureManagedLustreSkuInfo>();

            await foreach (var sku in sub.GetStorageCacheSkusAsync())
            {

                if (sku is null ||
                    !string.Equals(sku.ResourceType, "amlFilesystems", StringComparison.OrdinalIgnoreCase) ||
                    sku.LocationInfo is null ||
                    string.IsNullOrEmpty(sku.Name))
                    continue;

                var name = sku.Name;
                var capabilities = MapCapabilities(sku.Capabilities);

                foreach (var locationInfo in sku.LocationInfo)
                {
                    var foundLocation = locationInfo?.Location;
                    if (string.IsNullOrWhiteSpace(foundLocation) || (!string.IsNullOrWhiteSpace(location) && !string.Equals(foundLocation, location, StringComparison.OrdinalIgnoreCase)))
                        continue;
                    var supportsZones = (locationInfo?.Zones?.Count ?? 0) > 1;

                    results.Add(new AzureManagedLustreSkuInfo(name, foundLocation, supportsZones, [.. capabilities]));
                }
            }

            return results;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Azure Managed Lustre SKUs for subscription '{subscription}': {ex.Message}", ex);
        }
    }
}
