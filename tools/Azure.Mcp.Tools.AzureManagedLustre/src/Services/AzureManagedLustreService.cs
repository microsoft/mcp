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

    public async Task<List<AzureManagedLustreSkuInfo>> GetSkuInfoAsync(
        string subscription,
        string? tenant = null,
        string? region = null,
        RetryPolicyOptions? retryPolicy = null
        )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);

        var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");

        try
        {
            var results = new List<AzureManagedLustreSkuInfo>();
            await foreach (var sku in sub.GetStorageCacheSkusAsync())
            {
                if (sku is null)
                    continue;

                var resourceType = sku.ResourceType ?? string.Empty;


                if (resourceType != "amlFilesystems")
                    continue;


                var name = sku.Name ?? string.Empty;

                var capabilities = new List<AzureManagedLustreSkuCapability>();

                foreach (var capability in sku.Capabilities ?? [])
                {
                    var capName = capability?.Name ?? string.Empty;
                    var capValue = capability?.Value ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(capName))
                    {
                        capabilities.Add(new AzureManagedLustreSkuCapability(capName, capValue));
                    }
                }


                if (sku.LocationInfo is not null)
                {
                    foreach (var locationInfo in sku.LocationInfo)
                    {
                        var location = locationInfo?.Location;
                        if (string.IsNullOrWhiteSpace(location))
                            continue;

                        if (!string.IsNullOrWhiteSpace(region) && !string.Equals(location, region, StringComparison.OrdinalIgnoreCase))
                            continue;

                        bool supportsZones = false;

                        var zones = locationInfo?.Zones;
                        supportsZones = zones != null && zones.Count > 1;

                        results.Add(new AzureManagedLustreSkuInfo(name, location, supportsZones, new List<AzureManagedLustreSkuCapability>(capabilities)));
                    }
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
