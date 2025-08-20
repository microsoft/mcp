// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Postgres.Options.Database;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands.Database;

public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger) : BaseServerCommand<DatabaseListOptions>(logger)
{
    private const string CommandTitle = "List PostgreSQL Databases";

    public override string Name => "list";

    public override string Description => "Lists all databases in the PostgreSQL server.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var options = BindOptions(parseResult);
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            IPostgresService pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");
            List<string> databases = await pgService.ListDatabasesAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!);
            context.Response.Results = databases?.Count > 0 ?
                ResponseResult.Create(
                    new DatabaseListCommandResult(databases),
                    PostgresJsonContext.Default.DatabaseListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing databases.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record DatabaseListCommandResult(List<string> Databases);
}
