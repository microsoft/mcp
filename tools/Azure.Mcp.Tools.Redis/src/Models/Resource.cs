// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Mcp.Core.Models.Identity;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;

namespace Azure.Mcp.Tools.Redis.Models
{
    [JsonSourceGenerationOptions
        (
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase
    )]
    public class Resource
    {
        /// <summary> Name of the Redis resource. </summary>
        public string? Name { get; set; }

        /// <summary> Type of the Redis resource, either AzureManagedRedis or AzureCacheForRedis. </summary>
        public string? Type { get; set; }

        /// <summary> ID of the Azure subscription containing the Redis resource. </summary>
        public string? SubscriptionId { get; set; }

        /// <summary> Name of the resource group containing the Redis resource. </summary>
        public string? ResourceGroupName { get; set; }

        /// <summary> Azure geo-location where the Redis resource lives. </summary>
        public string? Location { get; set; }

        /// <summary> SKU of the Redis resource. </summary>
        public string? Sku { get; set; }

        /// <summary> Current status of the Redis resource. </summary>
        public string? Status { get; set; }

        /// <summary> Version of Redis server supported by the Redis resource. </summary>
        public string? RedisVersion { get; set; }

        /// <summary> DNS host name clients use to connect to the Redis resource. </summary>
        public string? HostName { get; set; }

        /// <summary> Minimum version of TLS supported for client connections to this Redis resource. </summary>
        public string? MinimumTlsVersion { get; set; }

        /// <summary> Resource IDs of private links used for network-isolated client connections to the Redis resource. </summary>
        public string[]? PrivateEndpointConnections { get; set; }

        /// <summary> The availability zones in which the Redis resource is deployed (if specified). </summary>
        public string[]? Zones { get; set; }

        /// <summary> System-assigned managed identity of the Redis resource. </summary>
        public ManagedIdentityInfo? Identity { get; set; }

        /// <summary> Tags on the Redis resource. </summary>
        public IDictionary<string, string>? Tags { get; set; }

        /// *****************************************
        /// Azure Managed Redis-Specific Properties
        /// *****************************************

        /// <summary> Provisioning state of the Redis resource. </summary>
        public string? ProvisioningState { get; set; }

        /// <summary> Databases in the Redis resource. </summary>
        public Database[]? Databases { get; set; }

        /// *****************************************
        /// Azure Cache for Redis-Specific Properties
        /// *****************************************

        /// <summary> Port for TLS (aka SSL) client connections to the Redis resource. </summary>
        public int? SslPort { get; set; }

        /// <summary> Port for unencrypted client connections to the Redis resource. </summary>
        public int? UnencryptedPort { get; set; }

        /// <summary> Number of shards in a clustered Redis resource. </summary>
        public int? ShardCount { get; set; }

        /// <summary> When a Redis resource is VNet-injected this contains the Resource ID of the subnet. </summary>
        public string? SubnetId { get; set; }

        /// <summary> Indicates whether public network access is allowed for the Redis resource. </summary>
        public bool? PublicNetworkAccess { get; set; }

        /// <summary> Indicates whether connections are allowed on the unencrypted port for the Redis resource. </summary>
        public bool? EnableNonSslPort { get; set; }

        /// <summary> Indicates whether access key authentication is disabled for the Redis resource. </summary>
        public bool? IsAccessKeyAuthenticationDisabled { get; set; }

        /// <summary> Resource IDs of other Redis servers linked to this one for geo-replication. </summary>
        public string[]? LinkedServers { get; set; }

        /// <summary> Number of replica nodes per primary node within the Redis resource. </summary>
        public int? ReplicasPerPrimary { get; set; }

        /// <summary> Either 'Preview' to receive new versions of Redis service components sooner, or 'Stable' to be updated later (default). </summary>
        public string? UpdateChannel { get; set; }

        /// <summary> Zonal allocation policy determining how the Redis resource is distributed across availability zones (Automatic, UserDefined, or NoZones). </summary>
        public string? ZonalAllocationPolicy { get; set; }

        /// <summary> Indicates whether RDB (Redis Database Backup) is enabled for the Redis resource. </summary>
        public bool? IsRdbBackupEnabled { get; set; }

        /// <summary> Number of minutes between RDB backups. Valid values: (15, 30, 60, 360, 720, 1440). </summary>
        public string? RdbBackupFrequency { get; set; }

        /// <summary> Indicates the maximum number of snapshots for RDB backup. </summary>
        public int? RdbBackupMaxSnapshotCount { get; set; }

        /// <summary> Indicates whether AOF (Append Only File) backup is enabled for the Redis resource. </summary>
        public bool? IsAofBackupEnabled { get; set; }

        /// <summary> Number of megabytes of memory reserved for fragmentation per shard. </summary>
        public string? MaxFragmentationMemoryReserved { get; set; }

        /// <summary> The eviction strategy used when your data won't fit within the cache memory limit. </summary>
        public string? MaxMemoryPolicy { get; set; }

        /// <summary> Number of megabytes of memory reserved for non-cache usage per shard e.g. failover. </summary>
        public string? MaxMemoryReserved { get; set; }

        /// <summary> Number of megabytes of memory reserved for non-cache usage per shard e.g. failover. </summary>
        public string? MaxMemoryDelta { get; set; }

        /// <summary> Maximum number of client connections. </summary>
        public int? MaxClients { get; set; }

        /// <summary> The keyspace events which should be monitored. </summary>
        public string? NotifyKeyspaceEvents { get; set; }

        /// <summary> Preferred authentication method to communicate to storage account used for data archive, specify SAS or ManagedIdentity, default value is SAS. </summary>
        public string? PreferredDataArchiveAuthMethod { get; set; }

        /// <summary> Preferred authentication method to communicate to storage account used for data persistence, specify SAS or ManagedIdentity, default value is SAS. </summary>
        public string? PreferredDataPersistenceAuthMethod { get; set; }

        /// <summary> Zonal Configuration. </summary>
        public string? ZonalConfiguration { get; set; }

        /// <summary> Indicates whether client connection authentication is disabled. Setting this property to true will compromise security and is strongly discouraged. </summary>
        public string? AuthNotRequired { get; set; }

        /// <summary> SubscriptionId of the storage account for persistence (AOF/RDB) using ManagedIdentity. </summary>
        public string? StorageSubscriptionId { get; set; }

        /// <summary> Indicates whether Microsoft Entra ID authentication has been enabled for client connections to the Redis resource. </summary>
        public bool? IsEntraIDAuthEnabled { get; set; }

        /// <summary> Access policy assignments for the Redis resource. </summary>
        public AccessPolicyAssignment[]? AccessPolicyAssignments { get; set; }
    }
}
