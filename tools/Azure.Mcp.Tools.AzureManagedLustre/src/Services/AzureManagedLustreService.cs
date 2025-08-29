// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.AzureManagedLustre.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Resources.Models;
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
                foreach (var fs in sub.GetAmlFileSystems())
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
            data.StorageCapacityTiB.HasValue ? (long?)Convert.ToInt64(Math.Round(data.StorageCapacityTiB.Value)) : null,
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
            var sdkResult = sub.GetRequiredAmlFSSubnetsSize(fileSystemSizeContent);
            var numberOfRequiredIPs = sdkResult.Value.FilesystemSubnetSize ?? throw new Exception($"Failed to retrieve the number of IPs");
            return numberOfRequiredIPs;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving required subnet size: {ex.Message}", ex);
        }
    }

    public async Task<LustreFileSystem> CreateFileSystemAsync(
        string subscription,
        string resourceGroup,
        string name,
        string location,
        string sku,
        int sizeTiB,
        string subnetId,
        string zone,
        string maintenanceDay,
        string maintenanceTime,
        string? hsmContainer = null,
        string? hsmLogContainer = null,
        string? importPrefix = null,
        string? rootSquashMode = null,
        string? noSquashNidLists = null,
        long? squashUid = null,
        long? squashGid = null,
        bool enableCustomEncryption = false,
        string? keyUrl = null,
        string? sourceVaultId = null,
        string? userAssignedIdentityId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceGroup);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(location);
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(subnetId);

        var rg = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroup, tenant, retryPolicy)
            ?? throw new Exception($"Resource group '{resourceGroup}' not found");

        var data = new AmlFileSystemData(new AzureLocation(location))
        {
            SkuName = sku,
            StorageCapacityTiB = sizeTiB,
            FilesystemSubnet = subnetId
        };

        // Validate zone support for the specified location before adding
        try
        {
            var sub = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy)
            ?? throw new Exception($"Subscription '{subscription}' not found");

            bool? supportsZones = null;

            await foreach (var loc in sub.GetLocationsAsync())
            {
                if (loc.Name.Equals(location, StringComparison.OrdinalIgnoreCase) ||
                    loc.DisplayName.Equals(location, StringComparison.OrdinalIgnoreCase))
                {
                    supportsZones = (loc.AvailabilityZoneMappings?.Count ?? 0) > 0;
                    break;
                }
            }

            if (supportsZones == false && !string.Equals(zone, "1", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Location '{location}' does not support availability zones; only zone '1' is allowed.");
            }
            if (supportsZones == true)
            {
                // Zone is required by command; add to zones
                data.Zones.Add(zone);
            }

        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to validate availability zones for location '{location}': {ex.Message}", ex);
        }



        // HSM settings if provided
        if (!string.IsNullOrWhiteSpace(hsmContainer) || !string.IsNullOrWhiteSpace(hsmLogContainer) || !string.IsNullOrWhiteSpace(importPrefix))
        {
            if (string.IsNullOrWhiteSpace(hsmContainer) || string.IsNullOrWhiteSpace(hsmLogContainer))
            {
                throw new ArgumentException("Both hsm-container and hsm-log-container must be provided when specifying HSM settings.");
            }

            var hsmSettings = new AmlFileSystemHsmSettings(hsmContainer, hsmLogContainer);
            if (!string.IsNullOrWhiteSpace(importPrefix))
            {
                hsmSettings.ImportPrefix = importPrefix;
            }
            data.Hsm = new AmlFileSystemPropertiesHsm
            {
                Settings = hsmSettings
            };
        }

        MaintenanceDayOfWeekType dayEnum;

        if (!Enum.TryParse<MaintenanceDayOfWeekType>(maintenanceDay, true, out dayEnum))
        {
            throw new ArgumentException($"Invalid maintenance day '{maintenanceDay}'. Allowed values: Monday..Sunday");
        }

        data.MaintenanceWindow = new AmlFileSystemPropertiesMaintenanceWindow
        {
            DayOfWeek = dayEnum,
            TimeOfDayUTC = maintenanceTime
        };

        // Root squash: default to None if not provided; when not None, ensure required squash parameters are provided

        if (!string.IsNullOrWhiteSpace(rootSquashMode))
        {
            AmlFileSystemSquashMode modeParsed = rootSquashMode;

            // When a squash mode other than None is specified, UID and GID must be provided
            if (modeParsed != AmlFileSystemSquashMode.None)
            {
                if (!squashUid.HasValue)
                {
                    throw new ArgumentException("squash-uid must be provided when root-squash-mode is not None.");
                }
                if (!squashGid.HasValue)
                {
                    throw new ArgumentException("squash-gid must be provided when root-squash-mode is not None.");
                }
                if (squashUid.Value < 0)
                {
                    throw new ArgumentException("squash-uid must be a non-negative integer.");
                }
                if (squashGid.Value < 0)
                {
                    throw new ArgumentException("squash-gid must be a non-negative integer.");
                }

                data.RootSquashSettings = new AmlFileSystemRootSquashSettings
                {
                    Mode = modeParsed,
                    NoSquashNidLists = modeParsed == AmlFileSystemSquashMode.None ? "" : noSquashNidLists,
                    SquashUID = modeParsed == AmlFileSystemSquashMode.None ? null : squashUid,
                    SquashGID = modeParsed == AmlFileSystemSquashMode.None ? null : squashGid
                };
            }
        }
        else
        {
            data.RootSquashSettings = new AmlFileSystemRootSquashSettings
            {
                Mode = AmlFileSystemSquashMode.None
            };
        }

        // Encryption
        if (enableCustomEncryption)
        {
            if (string.IsNullOrWhiteSpace(keyUrl) || string.IsNullOrWhiteSpace(sourceVaultId))
            {
                throw new Exception("Both key-url and source-vault must be provided when custom-encryption is enabled.");
            }
            data.KeyEncryptionKey = new StorageCacheEncryptionKeyVaultKeyReference(
                new Uri(keyUrl!),
                new WritableSubResource { Id = new ResourceIdentifier(sourceVaultId!) });

            // Assign user-assigned managed identity for Key Vault access
            if (!string.IsNullOrWhiteSpace(userAssignedIdentityId))
            {
                data.Identity = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned)
                {
                    UserAssignedIdentities =
                    {
                        [new ResourceIdentifier(userAssignedIdentityId)] = new UserAssignedIdentity()
                    }
                };

            }
        }

        try
        {
            var collection = rg.GetAmlFileSystems();
            var createOperationResult = await collection.CreateOrUpdateAsync(WaitUntil.Completed, name, data);
            var fileSystemResource = createOperationResult.Value;
            return Map(fileSystemResource);
        }
        catch (RequestFailedException rfe)
        {
            throw new Exception($"Failed to create AML file system '{name}': {rfe.Message}", rfe);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to create AML file system '{name}': {ex.Message}", ex);
        }
    }
}
