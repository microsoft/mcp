// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;

namespace Azure.Mcp.Tools.Redis.Models.CacheForRedis;

public class Cache
{
    /// <summary> Name of the Redis cache resource. </summary>
    public string? Name { get; set; }

    /// <summary> Name of the resource group containing the Redis cache resource. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> ID of the Azure subscription containing the Redis cache resource. </summary>
    public string? SubscriptionId { get; set; }

    /// <summary> Azure geo-location where the Redis cache resource lives. </summary>
    public string? Location { get; set; }

    /// <summary> SKU of the Redis cache resource. </summary>
    public string? Sku { get; set; }

    /// <summary> Provisioning status of the Redis cache resource. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> Version of Redis server supported by the cache. </summary>
    public string? RedisVersion { get; set; }

    /// <summary> DNS host name clients use to connect to the Redis cache. </summary>
    public string? HostName { get; set; }

    /// <summary> Port for TLS (aka SSL) client connections to the Redis cache. </summary>
    public int? SslPort { get; set; }

    /// <summary> Port for unencrypted client connections to the Redis cache. </summary>
    public int? Port { get; set; }

    /// <summary> Number of shards in a clustered Redis cache. </summary>
    public int? ShardCount { get; set; }

    /// <summary> When a Redis cache is VNet-injected this contains the Resource ID of the subnet. </summary>
    public string? SubnetId { get; set; }

    /// <summary> Indicates whether public network access is allowed for the Redis cache. </summary>
    public bool? PublicNetworkAccess { get; set; }

    /// <summary> Indicates whether connections are allowed on the non-SSL port for the Redis cache. </summary>
    public bool? EnableNonSslPort { get; set; }

    /// <summary> Indicates whether access key authentication is disabled for the Redis cache. </summary>
    public bool? IsAccessKeyAuthenticationDisabled { get; set; }

    /// <summary> Resource IDs of other Redis servers linked to this one for geo-replication. </summary>
    public string[]? LinkedServers { get; set; }

    /// <summary> Minimum version of TLS supported for client connections to this Redis cache. </summary>
    public string? MinimumTlsVersion { get; set; }

    /// <summary> Resource IDs of private links used for network-isolated client connections to the Redis cache. </summary>
    public string[]? PrivateEndpointConnections { get; set; }

    /// <summary> Number of replica nodes per primary node within the Redis cache. </summary>
    public int? ReplicasPerPrimary { get; set; }

    /// <summary> Either 'Preview' to receive new versions of Redis service components sooner, or 'Stable' to be updated later (default). </summary>
    public string? UpdateChannel { get; set; }

    /// <summary> Zonal allocation policy determining how the cache is distributed across availability zones. </summary>
    public string? ZonalAllocationPolicy { get; set; }

    /// <summary> The availability zones in which the Redis cache is deployed. </summary>
    public string[]? Zones { get; set; }

    /// <summary> Configuration settings for the Redis cache. </summary>
    public CacheConfiguration? Configuration { get; set; }

    /// <summary> System-assigned managed identity of the Redis cache resource. </summary>
    public ManagedIdentityInfo? Identity { get; set; }

    /// <summary> Tags on the Redis cache resource. </summary>
    public IDictionary<string, string>? Tags { get; set; }

}
