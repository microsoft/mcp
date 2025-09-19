// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Postgres.Options.Server;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands.Server;

public sealed class ServerConfigGetCommand(ILogger<ServerConfigGetCommand> logger) : BaseServerCommand<ServerConfigGetOptions>(logger)
{
    private const string CommandTitle = "Get PostgreSQL Server Configuration";
    public override string Name => "config";
    public override string Description =>
        "Retrieve the configuration of a PostgreSQL server.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
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

            IPostgresService pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");
            var config = await pgService.GetServerConfigAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!);
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
