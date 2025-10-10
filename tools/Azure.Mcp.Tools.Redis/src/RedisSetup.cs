// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Redis;

public class RedisSetup : IAreaSetup
{
    public string Name => "redis";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRedisService, RedisService>();

        services.AddSingleton<ResourceListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var redis = new CommandGroup(Name, "Redis operations - Commands for managing Azure Redis resources. Includes operations for listing Redis resources, databases, and data access policies, in both the Azure Managed Redis and legacy Azure Cache for Redis services.");

        var redisResourceList = serviceProvider.GetRequiredService<ResourceListCommand>();
        redis.AddCommand(redisResourceList.Name, redisResourceList);

        return redis;
    }
}
