// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Redis.Commands.CacheForRedis;
using Azure.Mcp.Tools.Redis.Commands.ManagedRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Redis;

public class RedisSetup : IAreaSetup
{
    public string Name => "redis";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRedisService, RedisService>();

        services.AddSingleton<CacheListCommand>();
        services.AddSingleton<AccessPolicyListCommand>();
        services.AddSingleton<ClusterListCommand>();
        services.AddSingleton<DatabaseListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var redis = new CommandGroup(Name, "Redis Cache operations - Commands for managing Azure Redis Cache and Azure Managed Redis resources. Includes operations for listing cache instances, managing clusters and databases, configuring access policies, and working with both traditional Redis Cache and Managed Redis services.");

        // Azure Cache for Redis
        var cache = new CommandGroup("cache", "Redis Cache resource operations - Commands for listing and managing Redis Cache resources in your Azure subscription.");
        redis.AddSubGroup(cache);

        var cacheList = serviceProvider.GetRequiredService<CacheListCommand>();
        cache.AddCommand(cacheList.Name, cacheList);

        var accessPolicy = new CommandGroup("accesspolicy", "Redis Cluster database operations - Commands for listing and managing Redis Cluster databases in your Azure subscription.");
        cache.AddSubGroup(accessPolicy);

        var accessPolicyList = serviceProvider.GetRequiredService<AccessPolicyListCommand>();
        accessPolicy.AddCommand(accessPolicyList.Name, accessPolicyList);

        // Azure Managed Redis
        var cluster = new CommandGroup("cluster", "Redis Cluster resource operations - Commands for listing and managing Redis Cluster resources in your Azure subscription.");
        redis.AddSubGroup(cluster);

        var clusterList = serviceProvider.GetRequiredService<ClusterListCommand>();
        cluster.AddCommand(clusterList.Name, clusterList);

        var database = new CommandGroup("database", "Redis Cluster database operations - Commands for listing and managing Redis Cluster Databases in your Azure subscription.");
        cluster.AddSubGroup(database);

        var databaseList = serviceProvider.GetRequiredService<DatabaseListCommand>();
        database.AddCommand(databaseList.Name, databaseList);

        return redis;
    }
}
