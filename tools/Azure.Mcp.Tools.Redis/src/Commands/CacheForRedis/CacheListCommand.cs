// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Redis.Models.CacheForRedis;
using Azure.Mcp.Tools.Redis.Options.CacheForRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Redis.Commands.CacheForRedis;

/// <summary>
/// Lists Azure Cache for Redis resources (Basic, Standard, and Premium tier caches) in the specified subscription.
/// </summary>
public sealed class CacheListCommand(ILogger<CacheListCommand> logger) : SubscriptionCommand<CacheListOptions>()
{
    private const string CommandTitle = "List Redis Caches";
    private readonly ILogger<CacheListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        List all Redis Cache resources in a specified subscription. Returns an array of Redis Cache details.
        Use this command to explore which Redis Cache resources are available in your subscription.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var redisService = context.GetService<IRedisService>() ?? throw new InvalidOperationException("Redis service is not available.");
            var caches = await redisService.ListCachesAsync(
                options.Subscription!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(caches ?? []), RedisJsonContext.Default.CacheListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Redis Caches");

            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record CacheListCommandResult(IEnumerable<Cache> Caches);
}
