// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis.Models.ManagedRedis;

/// <summary>
/// Parameters needed to create a Redis resource via BICEP template.
/// </summary>
public class RedisCreateParameters
{
    /// <summary>
    /// The name of the Redis resource to create.
    /// </summary>
    public required BicepParameter ResourceName { get; init; }

    /// <summary>
    /// The Azure region/location where the Redis resource will be created.
    /// </summary>
    public required BicepParameter Location { get; init; }

    /// <summary>
    /// The SKU name for the Redis resource.
    /// </summary>
    public required BicepParameter SkuName { get; init; }

    /// <summary>
    /// Access keys authentication setting (Enabled or Disabled).
    /// </summary>
    public required BicepParameter AccessKeyAuthenticationEnabled { get; init; }

    /// <summary>
    /// List of Redis modules to enable.
    /// </summary>
    public required ModuleList Modules { get; init; }
}
