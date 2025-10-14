// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;
using Azure.ResourceManager.Redis;
using Azure.ResourceManager.Redis.Models;
using Azure.ResourceManager.RedisEnterprise;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Tools.Redis.Services;

public class RedisService(ISubscriptionService _subscriptionService, ITenantService _tenantService)
    : BaseAzureService(_tenantService), IRedisService
{
    public async Task<IEnumerable<Resource>> ListResourcesAsync(
        string subscription,
        string? tenant = null,
        AuthMethod? authMethod = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters((nameof(subscription), subscription));

        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, tenant, retryPolicy) ?? throw new Exception($"Subscription '{subscription}' not found");

            var resources = new List<Resource>();
            var resourcesTasks = new List<Task<IEnumerable<Resource>>>();

            try
            {
                resourcesTasks.Add(ListAcrResourcesAsync(subscriptionResource));
                resourcesTasks.Add(ListAmrResourcesAsync(subscriptionResource));
                await Task.WhenAll(resourcesTasks);
            }
            catch (Exception)
            { }
            finally
            {
                foreach (var resourceTask in resourcesTasks)
                {
                    if (resourceTask.Status == TaskStatus.RanToCompletion)
                    {
                        resources.AddRange(resourceTask.Result);
                    }
                }
            }

            return resources;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error retrieving Redis resources: {ex.Message}", ex);
        }
    }

    private async Task<IEnumerable<Resource>> ListAcrResourcesAsync(SubscriptionResource subscriptionResource)
    {
        var resources = new List<Resource>();

        await foreach (var acrResource in subscriptionResource.GetAllRedisAsync())
        {
            if (string.IsNullOrWhiteSpace(acrResource?.Id.ToString())
                || string.IsNullOrWhiteSpace(acrResource.Data.Name))
            {
                continue;
            }

            var resource = acrResource.Data;
            var accessPolicyAssignments = new List<AccessPolicyAssignment>();

            try
            {
                var accessPolicyAssignmentCollection = acrResource.GetRedisCacheAccessPolicyAssignments();
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
            }
            catch (Exception) { }

            resources.Add(new()
            {
                Name = resource.Name,
                Type = "AzureCacheForRedis",
                ResourceGroupName = acrResource.Id.ResourceGroupName,
                SubscriptionId = acrResource.Id.SubscriptionId,
                Location = resource.Location,
                Sku = $"{resource.Sku.Name} {resource.Sku.Family}{resource.Sku.Capacity}",
                Status = resource.ProvisioningState?.ToString(),
                RedisVersion = resource.RedisVersion,
                HostName = resource.HostName,
                SslPort = resource.SslPort,
                UnencryptedPort = resource.Port,
                ShardCount = resource.ShardCount,
                PublicNetworkAccess = resource.PublicNetworkAccess?.Equals(RedisPublicNetworkAccess.Enabled),
                EnableNonSslPort = resource.EnableNonSslPort,
                IsAccessKeyAuthenticationDisabled = resource.IsAccessKeyAuthenticationDisabled,
                LinkedServers = resource.LinkedServers.Any() ?
                    [.. resource.LinkedServers.Select(server => server.Id.ToString())]
                    : null,
                MinimumTlsVersion = resource.MinimumTlsVersion.ToString(),
                PrivateEndpointConnections = resource.PrivateEndpointConnections.Any() ?
                    [.. resource.PrivateEndpointConnections.Select(connection => connection.Id.ToString())]
                    : null,
                Identity = resource.Identity is null ? null : new ManagedIdentityInfo
                {
                    SystemAssignedIdentity = new SystemAssignedIdentityInfo
                    {
                        Enabled = resource.Identity != null,
                        TenantId = resource.Identity?.TenantId?.ToString(),
                        PrincipalId = resource.Identity?.PrincipalId?.ToString()
                    },
                    UserAssignedIdentities = resource.Identity?.UserAssignedIdentities?
                        .Select(identity => new UserAssignedIdentityInfo
                        {
                            ClientId = identity.Value.ClientId?.ToString(),
                            PrincipalId = identity.Value.PrincipalId?.ToString()
                        }).ToArray()
                },
                ReplicasPerPrimary = resource.ReplicasPerPrimary,
                SubnetId = resource.SubnetId,
                UpdateChannel = resource.UpdateChannel?.ToString(),
                ZonalAllocationPolicy = resource.ZonalAllocationPolicy?.ToString(),
                Zones = resource.Zones?.Any() == true ? [.. resource.Zones] : null,
                Tags = resource.Tags.Any() ? resource.Tags : null,
                AccessPolicyAssignments = accessPolicyAssignments.Any() == true ? accessPolicyAssignments.ToArray() : null,
                AuthNotRequired = resource.RedisConfiguration.AuthNotRequired,
                IsRdbBackupEnabled = resource.RedisConfiguration.IsRdbBackupEnabled,
                IsAofBackupEnabled = resource.RedisConfiguration.IsAofBackupEnabled,
                RdbBackupFrequency = resource.RedisConfiguration.RdbBackupFrequency,
                RdbBackupMaxSnapshotCount = resource.RedisConfiguration.RdbBackupMaxSnapshotCount,
                MaxFragmentationMemoryReserved = resource.RedisConfiguration.MaxFragmentationMemoryReserved,
                MaxMemoryPolicy = resource.RedisConfiguration.MaxMemoryPolicy,
                MaxMemoryReserved = resource.RedisConfiguration.MaxMemoryReserved,
                MaxMemoryDelta = resource.RedisConfiguration.MaxMemoryDelta,
                MaxClients = int.TryParse(resource.RedisConfiguration.MaxClients.ToString(), out var maxClients) ? maxClients : null,
                NotifyKeyspaceEvents = resource.RedisConfiguration.NotifyKeyspaceEvents,
                PreferredDataArchiveAuthMethod = resource.RedisConfiguration.PreferredDataArchiveAuthMethod,
                PreferredDataPersistenceAuthMethod = resource.RedisConfiguration.PreferredDataPersistenceAuthMethod,
                ZonalConfiguration = resource.RedisConfiguration.ZonalConfiguration,
                StorageSubscriptionId = resource.RedisConfiguration.StorageSubscriptionId,
                IsEntraIDAuthEnabled = string.IsNullOrWhiteSpace(resource.RedisConfiguration.IsAadEnabled) ? null : StringComparer.OrdinalIgnoreCase.Equals(resource.RedisConfiguration.IsAadEnabled, "True"),
            });
        }

        return resources;
    }

    private async Task<IEnumerable<Resource>> ListAmrResourcesAsync(SubscriptionResource subscriptionResource)
    {
        var resources = new List<Resource>();

        await foreach (var amrResource in subscriptionResource.GetRedisEnterpriseClustersAsync())
        {
            if (string.IsNullOrWhiteSpace(amrResource?.Id.ToString())
                || string.IsNullOrWhiteSpace(amrResource.Data.Name))
            {
                continue;
            }

            var resource = amrResource.Data;
            var databases = new List<Database>();

            try
            {
                var databaseCollection = amrResource.GetRedisEnterpriseDatabases();
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
                        ClusterName = resource.Name,
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
            }
            catch (Exception) { }

            resources.Add(new()
            {
                Name = resource.Name,
                Type = "AzureManagedRedis",
                ResourceGroupName = amrResource.Id.ResourceGroupName,
                SubscriptionId = amrResource.Id.SubscriptionId,
                Location = resource.Location,
                Sku = resource.Sku.Name.ToString(),
                ProvisioningState = resource.ProvisioningState?.ToString(),
                HostName = resource.HostName,
                RedisVersion = resource.RedisVersion,
                Status = resource.ResourceState.ToString(),
                MinimumTlsVersion = resource.MinimumTlsVersion.ToString(),
                PrivateEndpointConnections = resource.PrivateEndpointConnections.Any() ?
                    [.. resource.PrivateEndpointConnections.Select(connection => connection.Id.ToString())]
                    : null,
                Identity = resource.Identity is null ? null : new ManagedIdentityInfo
                {
                    SystemAssignedIdentity = new SystemAssignedIdentityInfo
                    {
                        Enabled = resource.Identity != null,
                        TenantId = resource.Identity?.TenantId?.ToString(),
                        PrincipalId = resource.Identity?.PrincipalId?.ToString()
                    },
                    UserAssignedIdentities = resource.Identity?.UserAssignedIdentities?
                        .Select(identity => new UserAssignedIdentityInfo
                        {
                            ClientId = identity.Value.ClientId?.ToString(),
                            PrincipalId = identity.Value.PrincipalId?.ToString()
                        }).ToArray()
                },
                Zones = resource.Zones?.Any() == true ? [.. resource.Zones] : null,
                Tags = resource.Tags.Any() ? resource.Tags : null,
                Databases = databases.Any() == true ? databases.ToArray() : null
            });
        }

        return resources;
    }
}
