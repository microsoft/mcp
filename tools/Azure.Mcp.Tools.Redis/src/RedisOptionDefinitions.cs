// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis;

public static class RedisOptionDefinitions
{
    public const string CacheName = "cache";
    public const string ClusterName = "cluster";

    public static readonly Option<string> Cache = new(
        $"--{CacheName}",
        "The name of the Redis cache (e.g., my-redis-cache)."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Cluster = new(
        $"--{ClusterName}",
        "The name of the Redis cluster (e.g., my-redis-cluster)."
    )
    {
        IsRequired = true
    };
}
