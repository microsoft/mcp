// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Redis.Models.ManagedRedis;
using Azure.Mcp.Tools.Redis.Options.ManagedRedis;
using Azure.Mcp.Tools.Redis.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Redis.Commands.ManagedRedis;

/// <summary>
/// Lists the databases in the specified Azure Managed Redis or Azure Redis Enterprise cluster.
/// </summary>
public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger) : BaseClusterCommand<DatabaseListOptions>()
{
    private const string CommandTitle = "List Redis Cluster Databases";
    private readonly ILogger<DatabaseListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        $"""
        List the databases in the specified Redis Cluster resource. Returns an array of Redis database details.
        Use this command to explore which databases are available in your Redis Cluster.
        """;
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

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
            var databases = await redisService.ListDatabasesAsync(
                options.Cluster!,
                options.ResourceGroup!,
                options.Subscription!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = databases.Any() ?
                ResponseResult.Create(
                    new DatabaseListCommandResult(databases),
                    RedisJsonContext.Default.DatabaseListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list Redis Databases");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record DatabaseListCommandResult(IEnumerable<Database> Databases);
}
