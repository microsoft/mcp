// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Redis;

public static class RedisOptionDefinitions
{
    public const string CacheName = "cache";
    public const string ClusterName = "cluster";

    public static readonly Option<string> Cache = new(
        $"--{CacheName}"
    )
    {
        Description = "The name of the Redis cache (e.g., my-redis-cache).",
        Required = true
    };

    public static readonly Option<string> Cluster = new(
        $"--{ClusterName}"
    )
    {
        Description = "The name of the Redis cluster (e.g., my-redis-cluster).",
        Required = true
    };
}
