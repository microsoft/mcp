// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands.Server;

public sealed class ServerConfigGetCommand(IPostgresService postgresService, ILogger<ServerConfigGetCommand> logger) : BaseServerCommand<ServerConfigGetOptions, ServerConfigGetCommand.ServerConfigGetCommandResult>(logger)
{
    private readonly IPostgresService _postgresService = postgresService;
    private const string CommandTitle = "Get PostgreSQL Server Configuration";

    public override string Id => "049a0d10-0a6e-4278-a0a3-15ce6b2e5ee1";

    public override string Name => "get";

    public override string Description =>
        "Retrieve the configuration of a PostgreSQL server.";

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

    protected override JsonTypeInfo<ServerConfigGetCommandResult> ResultTypeInfo => PostgresJsonContext.Default.ServerConfigGetCommandResult;

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
            if (config?.Length > 0)
            {
                SetResult(context, new(config));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving server configuration.");
            HandleException(context, ex);
        }
        return context.Response;
    }
    public record ServerConfigGetCommandResult(string Configuration);
}
