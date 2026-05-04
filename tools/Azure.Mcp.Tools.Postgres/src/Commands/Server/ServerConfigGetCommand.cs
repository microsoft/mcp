// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
public sealed class ServerConfigGetCommand(IPostgresService postgresService, ILogger<ServerConfigGetCommand> logger) : BaseServerCommand<ServerConfigGetOptions>(logger)
{
    private readonly IPostgresService _postgresService = postgresService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {

            var config = await _postgresService.GetServerConfigAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, cancellationToken);
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
    internal record ServerConfigGetCommandResult(string Configuration);
}
