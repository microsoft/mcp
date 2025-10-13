// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Options;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Redis.Commands;

/// <summary>
/// Lists Redis resources in a subscription. Returns details for all Azure Managed Redis, Azure Cache for Redis, and Azure Redis Enterprise resources.
/// </summary>
public sealed class ResourceListCommand(ILogger<ResourceListCommand> logger) : SubscriptionCommand<ResourceListOptions>()
{
    private const string CommandTitle = "List Redis Resources";
    private readonly ILogger<ResourceListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        List/show all Redis resources in a subscription. Returns details of all Azure Managed Redis, Azure Cache for Redis, and Azure Redis Enterprise resources. Use this command to explore and view which Redis resources are available in your subscription.
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
            var resources = await redisService.ListResourcesAsync(
                options.Subscription!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(resources ?? []), RedisJsonContext.Default.ResourceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Redis resources");

            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ResourceListCommandResult(IEnumerable<Resource> Resources);
}
