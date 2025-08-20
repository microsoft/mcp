// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;

namespace Azure.Mcp.Tools.Redis.Models.ManagedRedis;

public class Cluster
{
    /// <summary> Name of the Redis cluster resource. </summary>
    public string? Name { get; set; }

    /// <summary> ID of the Azure subscription containing the Redis cluster resource. </summary>
    public string? SubscriptionId { get; set; }

    /// <summary> Name of the resource group containing the Redis cluster resource. </summary>
    public string? ResourceGroupName { get; set; }

    /// <summary> Azure geo-location where the Redis cluster resource lives. </summary>
    public string? Location { get; set; }

    /// <summary> SKU of the Redis cluster resource. </summary>
    public string? Sku { get; set; }

    /// <summary> Provisioning status of the Redis cluster resource. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> Current status of the Redis cluster. </summary>
    public string? ResourceState { get; set; }

    /// <summary> Version of Redis server supported by the cluster. </summary>
    public string? RedisVersion { get; set; }

    /// <summary> DNS host name clients use to connect to the Redis cluster. </summary>
    public string? HostName { get; set; }

    /// <summary> Minimum version of TLS supported for client connections to this Redis cluster. </summary>
    public string? MinimumTlsVersion { get; set; }

    /// <summary> Resource IDs of private links used for network-isolated client connections to the Redis cluster. </summary>
    public string[]? PrivateEndpointConnections { get; set; }

    /// <summary> The availability zones in which the Redis cluster is deployed. </summary>
    public string[]? Zones { get; set; }

    /// <summary> System-assigned managed identity of the Redis cluster resource. </summary>
    public ManagedIdentityInfo? Identity { get; set; }

    /// <summary> Tags on the Redis cluster resource. </summary>
    public IDictionary<string, string>? Tags { get; set; }

}
