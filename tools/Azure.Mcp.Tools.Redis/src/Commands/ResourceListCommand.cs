// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Redis.Models;
using Azure.Mcp.Tools.Redis.Options;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Redis.Commands;

/// <summary>
/// Lists Redis resources in a subscription. Returns details for all Azure Managed Redis, Azure Cache for Redis, and Azure Redis Enterprise resources.
/// </summary>
[CommandMetadata(
    Id = "eded7479-4187-4742-957f-d7778e03a69d",
    Name = "list",
    Title = "List Redis Resources",
    Description = "List/show all Redis resources in a subscription. Returns details of all Azure Managed Redis, Azure Cache for Redis, and Azure Redis Enterprise resources. Use this command to explore and view which Redis resources are available in your subscription.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ResourceListCommand(IRedisService redisService, ILogger<ResourceListCommand> logger)
    : SubscriptionCommand<ResourceListOptions>()
{
    private readonly IRedisService _redisService = redisService;
    private readonly ILogger<ResourceListCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var resources = await _redisService.ListResourcesAsync(
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

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
