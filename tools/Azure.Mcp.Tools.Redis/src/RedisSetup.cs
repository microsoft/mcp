// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Redis.Commands;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Redis;

public class RedisSetup : IAreaSetup
{
    public string Name => "redis";

    public string Title => "Azure Redis";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IRedisService, RedisService>();

        services.AddSingleton<ResourceListCommand>();
        services.AddSingleton<ResourceCreateCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var redis = new CommandGroup(Name, "Redis operations – Commands to manage Azure Redis resources, including creating and listing Redis instances, databases, and data access policies, across Azure Managed Redis and legacy Azure Cache for Redis services.", Title);

        var redisResourceList = serviceProvider.GetRequiredService<ResourceListCommand>();
        redis.AddCommand(redisResourceList.Name, redisResourceList);

        var redisResourceCreate = serviceProvider.GetRequiredService<ResourceCreateCommand>();
        redis.AddCommand(redisResourceCreate.Name, redisResourceCreate);

        return redis;
    }
}
