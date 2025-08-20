// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;
using Azure.ResourceManager.Redis;
using Azure.ResourceManager.Redis.Models;
using Azure.ResourceManager.RedisEnterprise;

namespace Azure.Mcp.Tools.Redis.Services;

public class RedisService(ISubscriptionService _subscriptionService, IResourceGroupService _resourceGroupService, ITenantService tenantService)
    : BaseAzureService(tenantService), IRedisService
{
    public async Task<IEnumerable<Cache>> ListCachesAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");
            var caches = new List<Cache>();

            await foreach (var cacheResource in subscriptionResource.GetAllRedisAsync())
            {
                if (string.IsNullOrWhiteSpace(cacheResource?.Id.ToString())
                    || string.IsNullOrWhiteSpace(cacheResource.Data.Name))
                {
                    continue;
                }

                var cache = cacheResource.Data;
                caches.Add(new()
                {
                    Name = cache.Name,
                    ResourceGroupName = cacheResource.Id.ResourceGroupName,
                    SubscriptionId = cacheResource.Id.SubscriptionId,
                    Location = cache.Location,
                    Sku = $"{cache.Sku.Name} {cache.Sku.Family}{cache.Sku.Capacity}",
                    ProvisioningState = cache.ProvisioningState?.ToString(),
                    RedisVersion = cache.RedisVersion,
                    HostName = cache.HostName,
                    SslPort = cache.SslPort,
                    Port = cache.Port,
                    ShardCount = cache.ShardCount,
                    PublicNetworkAccess = cache.PublicNetworkAccess?.Equals(RedisPublicNetworkAccess.Enabled),
                    EnableNonSslPort = cache.EnableNonSslPort,
                    IsAccessKeyAuthenticationDisabled = cache.IsAccessKeyAuthenticationDisabled,
                    LinkedServers = cache.LinkedServers.Any() ?
                        [.. cache.LinkedServers.Select(server => server.Id.ToString())]
                        : null,
                    MinimumTlsVersion = cache.MinimumTlsVersion.ToString(),
                    PrivateEndpointConnections = cache.PrivateEndpointConnections.Any() ?
                        [.. cache.PrivateEndpointConnections.Select(connection => connection.Id.ToString())]
                        : null,
                    Identity = cache.Identity is null ? null : new ManagedIdentityInfo
                    {
                        SystemAssignedIdentity = new SystemAssignedIdentityInfo
                        {
                            Enabled = cache.Identity != null,
                            TenantId = cache.Identity?.TenantId?.ToString(),
                            PrincipalId = cache.Identity?.PrincipalId?.ToString()
                        },
                        UserAssignedIdentities = cache.Identity?.UserAssignedIdentities?
                            .Select(identity => new UserAssignedIdentityInfo
                            {
                                ClientId = identity.Value.ClientId?.ToString(),
                                PrincipalId = identity.Value.PrincipalId?.ToString()
                            }).ToArray()
                    },
                    ReplicasPerPrimary = cache.ReplicasPerPrimary,
                    SubnetId = cache.SubnetId,
                    UpdateChannel = cache.UpdateChannel?.ToString(),
                    ZonalAllocationPolicy = cache.ZonalAllocationPolicy?.ToString(),
                    Zones = cache.Zones?.Any() == true ? [.. cache.Zones] : null,
                    Tags = cache.Tags.Any() ? cache.Tags : null,
                    Configuration = new()
                    {
                        AuthNotRequired = cache.RedisConfiguration.AuthNotRequired,
                        IsRdbBackupEnabled = cache.RedisConfiguration.IsRdbBackupEnabled,
                        IsAofBackupEnabled = cache.RedisConfiguration.IsAofBackupEnabled,
                        RdbBackupFrequency = cache.RedisConfiguration.RdbBackupFrequency,
                        RdbBackupMaxSnapshotCount = cache.RedisConfiguration.RdbBackupMaxSnapshotCount,
                        MaxFragmentationMemoryReserved = cache.RedisConfiguration.MaxFragmentationMemoryReserved,
                        MaxMemoryPolicy = cache.RedisConfiguration.MaxMemoryPolicy,
                        MaxMemoryReserved = cache.RedisConfiguration.MaxMemoryReserved,
                        MaxMemoryDelta = cache.RedisConfiguration.MaxMemoryDelta,
                        MaxClients = int.TryParse(cache.RedisConfiguration.MaxClients.ToString(), out var maxClients) ? maxClients : null,
                        NotifyKeyspaceEvents = cache.RedisConfiguration.NotifyKeyspaceEvents,
                        PreferredDataArchiveAuthMethod = cache.RedisConfiguration.PreferredDataArchiveAuthMethod,
                        PreferredDataPersistenceAuthMethod = cache.RedisConfiguration.PreferredDataPersistenceAuthMethod,
                        ZonalConfiguration = cache.RedisConfiguration.ZonalConfiguration,
                        StorageSubscriptionId = cache.RedisConfiguration.StorageSubscriptionId,
                        IsEntraIDAuthEnabled = string.IsNullOrWhiteSpace(cache.RedisConfiguration.IsAadEnabled) ? null : StringComparer.OrdinalIgnoreCase.Equals(cache.RedisConfiguration.IsAadEnabled, "True"),
                    }
                });
            }

            return caches;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Redis caches: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<AccessPolicyAssignment>> ListAccessPolicyAssignmentsAsync(
        string cacheName,
        string resourceGroupName,
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(cacheName, resourceGroupName, subscription);

        try
        {
            var resourceGroup = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroupName, tenant, retryPolicy) ?? throw new Exception($"Resource group named '{resourceGroupName}' not found");
            var cacheResponse = await resourceGroup.GetRedisAsync(cacheName);
            var accessPolicyAssignmentCollection = cacheResponse.Value.GetRedisCacheAccessPolicyAssignments();
            var accessPolicyAssignments = new List<AccessPolicyAssignment>();

            await foreach (var accessPolicyAssignmentResource in accessPolicyAssignmentCollection)
            {
                if (string.IsNullOrWhiteSpace(accessPolicyAssignmentResource?.Id.ToString())
                    || string.IsNullOrWhiteSpace(accessPolicyAssignmentResource.Data.Name))
                {
                    continue;
                }

                var accessPolicyAssignment = accessPolicyAssignmentResource.Data;
                accessPolicyAssignments.Add(new()
                {
                    AccessPolicyName = accessPolicyAssignment.AccessPolicyName,
                    IdentityName = accessPolicyAssignment.ObjectIdAlias,
                    ProvisioningState = accessPolicyAssignment.ProvisioningState?.ToString(),
                });
            }

            return accessPolicyAssignments;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Redis cache access policy assignments: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Cluster>> ListClustersAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(subscription);

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");
            var clusters = new List<Cluster>();

            await foreach (var clusterResource in subscriptionResource.GetRedisEnterpriseClustersAsync())
            {
                if (string.IsNullOrWhiteSpace(clusterResource?.Id.ToString())
                    || string.IsNullOrWhiteSpace(clusterResource.Data.Name))
                {
                    continue;
                }

                var cluster = clusterResource.Data;
                clusters.Add(new()
                {
                    Name = cluster.Name,
                    ResourceGroupName = clusterResource.Id.ResourceGroupName,
                    SubscriptionId = clusterResource.Id.SubscriptionId,
                    Location = cluster.Location,
                    Sku = cluster.Sku.Name.ToString(),
                    ProvisioningState = cluster.ProvisioningState?.ToString(),
                    HostName = cluster.HostName,
                    RedisVersion = cluster.RedisVersion,
                    ResourceState = cluster.ResourceState.ToString(),
                    MinimumTlsVersion = cluster.MinimumTlsVersion.ToString(),
                    PrivateEndpointConnections = cluster.PrivateEndpointConnections.Any() ?
                        [.. cluster.PrivateEndpointConnections.Select(connection => connection.Id.ToString())]
                        : null,
                    Identity = cluster.Identity is null ? null : new ManagedIdentityInfo
                    {
                        SystemAssignedIdentity = new SystemAssignedIdentityInfo
                        {
                            Enabled = cluster.Identity != null,
                            TenantId = cluster.Identity?.TenantId?.ToString(),
                            PrincipalId = cluster.Identity?.PrincipalId?.ToString()
                        },
                        UserAssignedIdentities = cluster.Identity?.UserAssignedIdentities?
                            .Select(identity => new UserAssignedIdentityInfo
                            {
                                ClientId = identity.Value.ClientId?.ToString(),
                                PrincipalId = identity.Value.PrincipalId?.ToString()
                            }).ToArray()
                    },
                    Zones = cluster.Zones?.Any() == true ? [.. cluster.Zones] : null,
                    Tags = cluster.Tags.Any() ? cluster.Tags : null,
                });
            }

            return clusters;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Redis clusters: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<Database>> ListDatabasesAsync(
        string clusterName,
        string resourceGroupName,
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(clusterName, resourceGroupName, subscription);

        try
        {
            var resourceGroup = await _resourceGroupService.GetResourceGroupResource(subscription, resourceGroupName, tenant, retryPolicy) ?? throw new Exception($"Resource group named '{resourceGroupName}' not found");
            var clusterResponse = await resourceGroup.GetRedisEnterpriseClusterAsync(clusterName);
            var databaseCollection = clusterResponse.Value.GetRedisEnterpriseDatabases();
            var databases = new List<Database>();

            await foreach (var databaseResource in databaseCollection)
            {
                if (string.IsNullOrWhiteSpace(databaseResource?.Id.ToString())
                    || string.IsNullOrWhiteSpace(databaseResource.Data.Name))
                {
                    continue;
                }

                var database = databaseResource.Data;
                databases.Add(new()
                {
                    Name = database.Name,
                    ClusterName = clusterName,
                    ResourceGroupName = databaseResource.Id.ResourceGroupName,
                    SubscriptionId = databaseResource.Id.SubscriptionId,
                    ProvisioningState = database.ProvisioningState?.ToString(),
                    ResourceState = database.ResourceState?.ToString(),
                    ClientProtocol = database.ClientProtocol?.ToString(),
                    Port = database.Port,
                    ClusteringPolicy = database.ClusteringPolicy?.ToString(),
                    EvictionPolicy = database.EvictionPolicy?.ToString(),
                    IsAofEnabled = database.Persistence?.IsAofEnabled,
                    AofFrequency = database.Persistence?.AofFrequency?.ToString(),
                    IsRdbEnabled = database.Persistence?.IsRdbEnabled,
                    RdbFrequency = database.Persistence?.RdbFrequency?.ToString(),
                    Modules = database.Modules?.Any() == true ? [.. database.Modules.Select(module => new Module() { Name = module.Name, Version = module.Version, Args = module.Args })] : null,
                    GeoReplicationGroupNickname = database.GeoReplication?.GroupNickname,
                    GeoReplicationLinkedDatabases = database.GeoReplication?.LinkedDatabases?.Any() == true ? [.. database.GeoReplication.LinkedDatabases.Select(linkedDatabase => $"{linkedDatabase.State}: {linkedDatabase.Id}")] : null,
                });
            }

            return databases;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Redis cluster databases: {ex.Message}", ex);
        }
    }
}
