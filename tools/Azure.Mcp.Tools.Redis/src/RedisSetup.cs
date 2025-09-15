// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Redis.Commands.CacheForRedis;
using Azure.Mcp.Tools.Redis.Commands.ManagedRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Redis;

public class RedisSetup : IAreaSetup
{
    public string Name => "redis";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRedisService, RedisService>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var redis = new CommandGroup(Name, "Redis Cache operations - Commands for managing Azure Redis Cache and Azure Managed Redis resources. Includes operations for listing cache instances, managing clusters and databases, configuring access policies, and working with both traditional Redis Cache and Managed Redis services.");
        rootGroup.AddSubGroup(redis);

        // Azure Cache for Redis
        var cache = new CommandGroup("cache", "Redis Cache resource operations - Commands for listing and managing Redis Cache resources in your Azure subscription.");
        redis.AddSubGroup(cache);

        cache.AddCommand("list", new CacheListCommand(loggerFactory.CreateLogger<CacheListCommand>()));

        var accessPolicy = new CommandGroup("accesspolicy", "Redis Cluster database operations - Commands for listing and managing Redis Cluster databases in your Azure subscription.");
        cache.AddSubGroup(accessPolicy);

        accessPolicy.AddCommand("list", new AccessPolicyListCommand(loggerFactory.CreateLogger<AccessPolicyListCommand>()));

        // Azure Managed Redis
        var cluster = new CommandGroup("cluster", "Redis Cluster resource operations - Commands for listing and managing Redis Cluster resources in your Azure subscription.");
        redis.AddSubGroup(cluster);

        cluster.AddCommand("list", new ClusterListCommand(loggerFactory.CreateLogger<ClusterListCommand>()));

        var database = new CommandGroup("database", "Redis Cluster database operations - Commands for listing and managing Redis Cluster Databases in your Azure subscription.");
        cluster.AddSubGroup(database);

        database.AddCommand("list", new DatabaseListCommand(loggerFactory.CreateLogger<DatabaseListCommand>()));
    }
}
