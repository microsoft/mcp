// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.StorageSync.Models;
using Azure.ResourceManager.StorageSync;
using Azure.ResourceManager.StorageSync.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.StorageSync.Services;

/// <summary>
/// Implementation of IStorageSyncService.
/// </summary>
public sealed class StorageSyncService(
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    ILogger<StorageSyncService> logger) : BaseAzureResourceService(subscriptionService, tenantService), IStorageSyncService
{
    private readonly ILogger<StorageSyncService> _logger = logger;

    public async Task<List<StorageSyncServiceDataSchema>> ListStorageSyncServicesAsync(
        string subscription,
        string? resourceGroup = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));

            var services = new List<StorageSyncServiceDataSchema>();

            if (!string.IsNullOrEmpty(resourceGroup))
            {
                Azure.ResourceManager.Resources.ResourceGroupResource resourceGroupResource;
                try
                {
                    var response = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
                    resourceGroupResource = response.Value;
                }
                catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
                {
                    _logger.LogWarning(reqEx,
                        "Resource group not found when listing Storage Sync services. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                        resourceGroup, subscription);
                    return [];
                }

                var collection = resourceGroupResource.GetStorageSyncServices();
                await foreach (var serviceResource in collection)
                {
                    services.Add(StorageSyncServiceDataSchema.FromResource(serviceResource));
                }
            }
            else
            {
                await foreach (var serviceResource in subscriptionResource.GetStorageSyncServicesAsync(cancellationToken))
                {
                    services.Add(StorageSyncServiceDataSchema.FromResource(serviceResource));
                }
            }

            return services;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing Storage Sync services. ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                resourceGroup, subscription);
            throw;
        }
    }

    public async Task<StorageSyncServiceDataSchema?> GetStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            return StorageSyncServiceDataSchema.FromResource(serviceResource.Value);
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx,
                "Storage Sync service not found. Service: {Service}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                storageSyncServiceName, resourceGroup, subscription);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                storageSyncServiceName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<StorageSyncServiceDataSchema> CreateStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string location,
        Dictionary<string, string>? tags = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(location), location)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);

            var content = new Azure.ResourceManager.StorageSync.Models.StorageSyncServiceCreateOrUpdateContent(new Azure.Core.AzureLocation(location));
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    content.Tags.Add(tag.Key, tag.Value);
                }
            }

            var operation = await resourceGroupResource.Value.GetStorageSyncServices().CreateOrUpdateAsync(
                WaitUntil.Completed,
                storageSyncServiceName,
                content,
                cancellationToken);

            _logger.LogInformation(
                "Successfully created Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}, Location: {Location}",
                storageSyncServiceName, resourceGroup, location);

            return StorageSyncServiceDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                storageSyncServiceName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<StorageSyncServiceDataSchema> UpdateStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? incomingTrafficPolicy = null,
        Dictionary<string, object>? tags = null,
        string? identityType = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            var patch = new Azure.ResourceManager.StorageSync.Models.StorageSyncServicePatch();

            // Update incoming traffic policy
            if (!string.IsNullOrEmpty(incomingTrafficPolicy))
            {
                patch.IncomingTrafficPolicy = new IncomingTrafficPolicy(incomingTrafficPolicy);
            }

            // Update tags
            if (tags != null)
            {
                foreach (var tag in tags)
                {
                    patch.Tags[tag.Key] = tag.Value?.ToString() ?? string.Empty;
                }
            }

            // Update identity
            if (!string.IsNullOrEmpty(identityType))
            {
                var identity = new Azure.ResourceManager.Models.ManagedServiceIdentity(
                    new Azure.ResourceManager.Models.ManagedServiceIdentityType(identityType));
                patch.Identity = identity;
            }

            var operation = await serviceResource.Value.UpdateAsync(WaitUntil.Completed, patch, cancellationToken);

            _logger.LogInformation(
                "Successfully updated Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}",
                storageSyncServiceName, resourceGroup);

            return StorageSyncServiceDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                storageSyncServiceName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task DeleteStorageSyncServiceAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            await serviceResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}",
                storageSyncServiceName, resourceGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting Storage Sync service. Service: {Service}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                storageSyncServiceName, resourceGroup, subscription);
            throw;
        }
    }

    // Sync Group Operations
    public async Task<List<SyncGroupDataSchema>> ListSyncGroupsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            var syncGroups = new List<SyncGroupDataSchema>();
            await foreach (var syncGroupResource in serviceResource.Value.GetStorageSyncGroups())
            {
                syncGroups.Add(SyncGroupDataSchema.FromResource(syncGroupResource));
            }

            return syncGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Sync Groups");
            throw;
        }
    }

    public async Task<SyncGroupDataSchema?> GetSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

            return SyncGroupDataSchema.FromResource(syncGroupResource.Value);
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Sync Group not found: {SyncGroup}", syncGroupName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Sync Group: {SyncGroup}", syncGroupName);
            throw;
        }
    }

    public async Task<SyncGroupDataSchema> CreateSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            var content = new Azure.ResourceManager.StorageSync.Models.StorageSyncGroupCreateOrUpdateContent();
            var operation = await serviceResource.Value.GetStorageSyncGroups().CreateOrUpdateAsync(WaitUntil.Completed, syncGroupName, content, cancellationToken);

            _logger.LogInformation("Successfully created Sync Group: {SyncGroup}", syncGroupName);
            return SyncGroupDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Sync Group: {SyncGroup}", syncGroupName);
            throw;
        }
    }

    public async Task DeleteSyncGroupAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

            await syncGroupResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation("Successfully deleted Sync Group: {SyncGroup}", syncGroupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Sync Group: {SyncGroup}", syncGroupName);
            throw;
        }
    }

    // Cloud Endpoint Operations
    public async Task<List<CloudEndpointDataSchema>> ListCloudEndpointsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

            var endpoints = new List<CloudEndpointDataSchema>();
            await foreach (var endpointResource in syncGroupResource.Value.GetCloudEndpoints())
            {
                endpoints.Add(CloudEndpointDataSchema.FromResource(endpointResource));
            }

            return endpoints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Cloud Endpoints");
            throw;
        }
    }

    public async Task<CloudEndpointDataSchema?> GetCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetCloudEndpoints().GetAsync(cloudEndpointName, cancellationToken);

            return CloudEndpointDataSchema.FromResource(endpointResource.Value);
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Cloud Endpoint not found: {Endpoint}", cloudEndpointName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Cloud Endpoint: {Endpoint}", cloudEndpointName);
            throw;
        }
    }

    public async Task<CloudEndpointDataSchema> CreateCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string storageAccountResourceId,
        string azureFileShareName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName),
            (nameof(storageAccountResourceId), storageAccountResourceId),
            (nameof(azureFileShareName), azureFileShareName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));

            // Get subscription data to access tenant ID
            var subscriptionData = await subscriptionResource.GetAsync(cancellationToken);

            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

            // Get the tenant ID - use provided tenant or get from subscription
            Guid? storageAccountTenantId = null;
            if (tenant != null && Guid.TryParse(tenant, out var parsedTenantId))
            {
                storageAccountTenantId = parsedTenantId;
            }
            else if (subscriptionData.Value.Data.TenantId.HasValue)
            {
                storageAccountTenantId = subscriptionData.Value.Data.TenantId.Value;
            }

            var content = new Azure.ResourceManager.StorageSync.Models.CloudEndpointCreateOrUpdateContent
            {
                StorageAccountResourceId = new Azure.Core.ResourceIdentifier(storageAccountResourceId),
                AzureFileShareName = azureFileShareName,
                StorageAccountTenantId = storageAccountTenantId
            };
            var operation = await syncGroupResource.Value.GetCloudEndpoints().CreateOrUpdateAsync(
                WaitUntil.Completed, cloudEndpointName, content, cancellationToken);

            _logger.LogInformation("Successfully created Cloud Endpoint: {Endpoint}", cloudEndpointName);
            return CloudEndpointDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Cloud Endpoint: {Endpoint}", cloudEndpointName);
            throw;
        }
    }

    public async Task DeleteCloudEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetCloudEndpoints().GetAsync(cloudEndpointName, cancellationToken);

            await endpointResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation("Successfully deleted Cloud Endpoint: {Endpoint}", cloudEndpointName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Cloud Endpoint: {Endpoint}", cloudEndpointName);
            throw;
        }
    }

    public async Task TriggerChangeDetectionAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string cloudEndpointName,
        string directoryPath,
        string? changeDetectionMode = null,
        IList<string>? paths = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(cloudEndpointName), cloudEndpointName),
            (nameof(directoryPath), directoryPath)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetCloudEndpoints().GetAsync(cloudEndpointName, cancellationToken);

            var content = new Azure.ResourceManager.StorageSync.Models.TriggerChangeDetectionContent
            {
                DirectoryPath = directoryPath
            };

            // Set change detection mode if provided
            if (!string.IsNullOrEmpty(changeDetectionMode))
            {
                content.ChangeDetectionMode = new Azure.ResourceManager.StorageSync.Models.ChangeDetectionMode(changeDetectionMode);
            }

            // Add paths if provided
            if (paths != null)
            {
                foreach (var path in paths)
                {
                    content.Paths.Add(path);
                }
            }

            await endpointResource.Value.TriggerChangeDetectionAsync(WaitUntil.Completed, content, cancellationToken);

            _logger.LogInformation("Successfully triggered change detection for Cloud Endpoint: {Endpoint}", cloudEndpointName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering change detection for Cloud Endpoint: {Endpoint}", cloudEndpointName);
            throw;
        }
    }

    // Server Endpoint Operations
    public async Task<List<ServerEndpointDataSchema>> ListServerEndpointsAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

            var endpoints = new List<ServerEndpointDataSchema>();
            await foreach (var endpointResource in syncGroupResource.Value.GetStorageSyncServerEndpoints())
            {
                endpoints.Add(ServerEndpointDataSchema.FromResource(endpointResource));
            }

            return endpoints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Server Endpoints");
            throw;
        }
    }

    public async Task<ServerEndpointDataSchema?> GetServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetStorageSyncServerEndpoints().GetAsync(serverEndpointName, cancellationToken);

            return ServerEndpointDataSchema.FromResource(endpointResource.Value);
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Server Endpoint not found: {Endpoint}", serverEndpointName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Server Endpoint: {Endpoint}", serverEndpointName);
            throw;
        }
    }

    public async Task<ServerEndpointDataSchema> CreateServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string serverResourceId,
        string serverLocalPath,
        bool? enableCloudTiering = null,
        int? volumeFreeSpacePercent = null,
        int? tierFilesOlderThanDays = null,
        string? localCacheMode = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName),
            (nameof(serverResourceId), serverResourceId),
            (nameof(serverLocalPath), serverLocalPath)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);

            var content = new Azure.ResourceManager.StorageSync.Models.StorageSyncServerEndpointCreateOrUpdateContent
            {
                ServerResourceId = new Azure.Core.ResourceIdentifier(serverResourceId),
                ServerLocalPath = serverLocalPath
            };

            if (enableCloudTiering.HasValue)
            {
                content.CloudTiering = enableCloudTiering.Value ? Azure.ResourceManager.StorageSync.Models.StorageSyncFeatureStatus.On : Azure.ResourceManager.StorageSync.Models.StorageSyncFeatureStatus.Off;
            }
            if (volumeFreeSpacePercent.HasValue)
            {
                content.VolumeFreeSpacePercent = volumeFreeSpacePercent;
            }
            if (tierFilesOlderThanDays.HasValue)
            {
                content.TierFilesOlderThanDays = tierFilesOlderThanDays;
            }
            if (!string.IsNullOrEmpty(localCacheMode))
            {
                content.LocalCacheMode = new LocalCacheMode(localCacheMode);
            }

            var operation = await syncGroupResource.Value.GetStorageSyncServerEndpoints().CreateOrUpdateAsync(
                WaitUntil.Completed, serverEndpointName, content, cancellationToken);

            _logger.LogInformation("Successfully created Server Endpoint: {Endpoint}", serverEndpointName);
            return ServerEndpointDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Server Endpoint: {Endpoint}", serverEndpointName);
            throw;
        }
    }

    public async Task<ServerEndpointDataSchema> UpdateServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        bool? cloudTiering = null,
        int? volumeFreeSpacePercent = null,
        int? tierFilesOlderThanDays = null,
        string? localCacheMode = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetStorageSyncServerEndpoints().GetAsync(serverEndpointName, cancellationToken);

            var patch = new Azure.ResourceManager.StorageSync.Models.StorageSyncServerEndpointPatch();
            if (cloudTiering.HasValue)
            {
                patch.CloudTiering = cloudTiering.Value ? Azure.ResourceManager.StorageSync.Models.StorageSyncFeatureStatus.On : Azure.ResourceManager.StorageSync.Models.StorageSyncFeatureStatus.Off;
            }
            if (volumeFreeSpacePercent.HasValue)
            {
                patch.VolumeFreeSpacePercent = volumeFreeSpacePercent;
            }
            if (tierFilesOlderThanDays.HasValue)
            {
                patch.TierFilesOlderThanDays = tierFilesOlderThanDays;
            }
            if (!string.IsNullOrEmpty(localCacheMode))
            {
                patch.LocalCacheMode = new LocalCacheMode(localCacheMode);
            }

            var operation = await endpointResource.Value.UpdateAsync(WaitUntil.Completed, patch, cancellationToken);

            _logger.LogInformation("Successfully updated Server Endpoint: {Endpoint}", serverEndpointName);
            return ServerEndpointDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Server Endpoint: {Endpoint}", serverEndpointName);
            throw;
        }
    }

    public async Task DeleteServerEndpointAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string syncGroupName,
        string serverEndpointName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(syncGroupName), syncGroupName),
            (nameof(serverEndpointName), serverEndpointName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var syncGroupResource = await serviceResource.Value.GetStorageSyncGroups().GetAsync(syncGroupName, cancellationToken);
            var endpointResource = await syncGroupResource.Value.GetStorageSyncServerEndpoints().GetAsync(serverEndpointName, cancellationToken);

            await endpointResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation("Successfully deleted Server Endpoint: {Endpoint}", serverEndpointName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Server Endpoint: {Endpoint}", serverEndpointName);
            throw;
        }
    }

    // Registered Server Operations
    public async Task<List<RegisteredServerDataSchema>> ListRegisteredServersAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName)
        );

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            var servers = new List<RegisteredServerDataSchema>();
            await foreach (var serverResource in serviceResource.Value.GetStorageSyncRegisteredServers())
            {
                servers.Add(RegisteredServerDataSchema.FromResource(serverResource));
            }

            return servers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing Registered Servers");
            throw;
        }
    }

    public async Task<RegisteredServerDataSchema?> GetRegisteredServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId)
        );

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var serverResource = await serviceResource.Value.GetStorageSyncRegisteredServers().GetAsync(serverGuid, cancellationToken);

            return RegisteredServerDataSchema.FromResource(serverResource.Value);
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == (int)HttpStatusCode.NotFound)
        {
            _logger.LogWarning(reqEx, "Registered Server not found: {Server}", registeredServerId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Registered Server: {Server}", registeredServerId);
            throw;
        }
    }

    public async Task<RegisteredServerDataSchema> RegisterServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId)
        );

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);

            var content = new Azure.ResourceManager.StorageSync.Models.StorageSyncRegisteredServerCreateOrUpdateContent();
            var operation = await serviceResource.Value.GetStorageSyncRegisteredServers().CreateOrUpdateAsync(
                WaitUntil.Completed, serverGuid, content, cancellationToken);

            _logger.LogInformation("Successfully registered Server: {Server}", registeredServerId);
            return RegisteredServerDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering Server: {Server}", registeredServerId);
            throw;
        }
    }

    public async Task<RegisteredServerDataSchema> UpdateServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        Dictionary<string, object>? properties = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId)
        );

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var serverResource = await serviceResource.Value.GetStorageSyncRegisteredServers().GetAsync(serverGuid, cancellationToken);

            var patch = new Azure.ResourceManager.StorageSync.Models.StorageSyncRegisteredServerPatch();
            // Add any patch-specific logic here if needed

            var operation = await serverResource.Value.UpdateAsync(WaitUntil.Completed, patch, cancellationToken);

            _logger.LogInformation("Successfully updated Registered Server: {Server}", registeredServerId);
            return RegisteredServerDataSchema.FromResource(operation.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Registered Server: {Server}", registeredServerId);
            throw;
        }
    }

    public async Task UnregisterServerAsync(
        string subscription,
        string resourceGroup,
        string storageSyncServiceName,
        string registeredServerId,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(
            (nameof(subscription), subscription),
            (nameof(resourceGroup), resourceGroup),
            (nameof(storageSyncServiceName), storageSyncServiceName),
            (nameof(registeredServerId), registeredServerId)
        );

        // Validate registeredServerId is a valid GUID
        var serverGuid = CheckGuid(registeredServerId, nameof(registeredServerId));

        try
        {
            var armClient = await CreateArmClientAsync(tenant, retryPolicy, null, cancellationToken);
            var subscriptionResource = armClient.GetSubscriptionResource(
                Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup, cancellationToken);
            var serviceResource = await resourceGroupResource.Value.GetStorageSyncServices().GetAsync(storageSyncServiceName, cancellationToken);
            var serverResource = await serviceResource.Value.GetStorageSyncRegisteredServers().GetAsync(serverGuid, cancellationToken);

            await serverResource.Value.DeleteAsync(WaitUntil.Completed, cancellationToken);

            _logger.LogInformation("Successfully unregistered Server: {Server}", registeredServerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering Server: {Server}", registeredServerId);
            throw;
        }
    }

    /// <summary>
    /// Validates and converts a string to a GUID.
    /// </summary>
    /// <param name="value">The string value to convert</param>
    /// <param name="paramName">The parameter name for error messages</param>
    /// <returns>The converted GUID</returns>
    /// <exception cref="ArgumentException">Thrown if the value is not a valid GUID</exception>
    private static Guid CheckGuid(string value, string paramName)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            throw new ArgumentException($"'{paramName}' must be a valid GUID.", paramName);
        }

        return guid;
    }
}
