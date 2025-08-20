// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis.Models.CacheForRedis;

public class CacheConfiguration
{
    /// <summary> Indicates whether RDB (Redis Database Backup) is enabled for the Redis cache. </summary>
    public bool? IsRdbBackupEnabled { get; set; }

    /// <summary> Number of minutes between RDB backups. Valid values: (15, 30, 60, 360, 720, 1440). </summary>
    public string? RdbBackupFrequency { get; set; }

    /// <summary> Indicates the maximum number of snapshots for RDB backup. </summary>
    public int? RdbBackupMaxSnapshotCount { get; set; }

    /// <summary> Indicates whether AOF (Append Only File) backup is enabled for the Redis cache. </summary>
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

    /// <summary> Indicates whether client connection authentication is disabled. Setting this property is highly discouraged from security point of view. </summary>
    public string? AuthNotRequired { get; set; }

    /// <summary> SubscriptionId of the storage account for persistence (AOF/RDB) using ManagedIdentity. </summary>
    public string? StorageSubscriptionId { get; set; }

    /// <summary> Indicates whether Microsoft Entra ID authentication has been enabled for client connections to the cache. </summary>
    public bool? IsEntraIDAuthEnabled { get; set; }
}
