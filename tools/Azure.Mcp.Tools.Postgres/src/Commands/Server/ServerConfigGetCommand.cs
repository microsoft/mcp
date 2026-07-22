// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands.Server;

[CommandMetadata(
    Id = "049a0d10-0a6e-4278-a0a3-15ce6b2e5ee1",
    Name = "get",
    Title = "Get PostgreSQL Server Configuration",
    Description = "Retrieve the configuration of a PostgreSQL server.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ServerConfigGetCommand(IPostgresService postgresService, ILogger<ServerConfigGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BaseServerOptions, ServerConfigGetCommand.ServerConfigGetCommandResult>(subscriptionResolver)
{
    private readonly IPostgresService _postgresService = postgresService;
    private readonly ILogger<ServerConfigGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BaseServerOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var config = await _postgresService.GetServerConfigAsync(options.Subscription!, options.ResourceGroup, options.User, options.Server, options.Tenant, options.RetryPolicy, cancellationToken);
            context.Response.Results = config?.Length > 0 ?
                ResponseResult.Create(new(config), PostgresJsonContext.Default.ServerConfigGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving server configuration.");
            HandleException(context, ex);
        }
        return context.Response;
    }
    public sealed record ServerConfigGetCommandResult(string Configuration);
}
