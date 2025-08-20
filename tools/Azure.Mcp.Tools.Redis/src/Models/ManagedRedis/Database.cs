// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis.Models.ManagedRedis;

public class Database
{
    /// <summary> Name of the Redis cluster resource. </summary>
    public string? Name { get; set; }

    /// <summary> Name of the Redis cluster containing this database. </summary>
    public string? ClusterName { get; set; }

    /// <summary> Name of the resource group containing the Redis cluster that contains this database. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> ID of the Azure subscription containing the Redis cluster that contains this database. </summary>
    public string? SubscriptionId { get; set; }

    /// <summary> Specifies whether redis clients can connect using TLS-encrypted or plaintext redis protocols. Default is TLS-encrypted. </summary>
    public string? ClientProtocol { get; set; }

    /// <summary> TCP port of the database endpoint. Specified at create time. Defaults to an available port. </summary>
    public int? Port { get; set; }

    /// <summary> Provisioning status of the Redis cluster resource. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> Current status of the Redis cluster. </summary>
    public string? ResourceState { get; set; }

    /// <summary> Clustering policy - default is OSSCluster. Specified at create time. </summary>
    public string? ClusteringPolicy { get; set; }

    /// <summary> Redis eviction policy - default is VolatileLRU. </summary>
    public string? EvictionPolicy { get; set; }

    /// <summary> Sets whether AOF is enabled. </summary>
    public bool? IsAofEnabled { get; set; }

    /// <summary> Sets whether RDB is enabled. </summary>
    public bool? IsRdbEnabled { get; set; }

    /// <summary> Sets the frequency at which data is written to disk. </summary>
    public string? AofFrequency { get; set; }

    /// <summary> Sets the frequency at which a snapshot of the database is created. </summary>
    public string? RdbFrequency { get; set; }

    /// <summary> Optional set of redis modules to enable in this database - modules can only be added at creation time. </summary>
    public Module[]? Modules { get; set; }

    /// <summary> Name for the group of geo-linked database resources. </summary>
    public string? GeoReplicationGroupNickname { get; set; }

    /// <summary> List of databases linked with this database for geo-replication. </summary>
    public string[]? GeoReplicationLinkedDatabases { get; set; }

}
