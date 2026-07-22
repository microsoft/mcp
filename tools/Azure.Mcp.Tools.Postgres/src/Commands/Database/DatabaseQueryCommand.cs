// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Options.Database;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands.Database;

[CommandMetadata(
    Id = "81a28bca-014c-4738-9e1a-654d77cb2dd8",
    Name = "query",
    Title = "Query PostgreSQL Database",
    Description = "Executes a SQL query on an Azure Database for PostgreSQL server to search for specific terms, retrieve records, or perform SELECT operations.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseQueryCommand(IPostgresService postgresService, ILogger<DatabaseQueryCommand> logger)
    : AuthenticatedCommand<DatabaseQueryOptions, DatabaseQueryCommand.DatabaseQueryCommandResult>
{
    private readonly IPostgresService _postgresService = postgresService;
    private readonly ILogger<DatabaseQueryCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Validate the query early to avoid sending unsafe SQL to the server.
            SqlQueryValidator.EnsureReadOnlySelect(options.Query);
            List<string> queryResult = await _postgresService.ExecuteQueryAsync(
                options.AuthType,
                options.User,
                options.Password,
                options.Server,
                options.Database,
                options.Query,
                cancellationToken);
            context.Response.Results = ResponseResult.Create(new(queryResult ?? []), PostgresJsonContext.Default.DatabaseQueryCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while executing the query.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DatabaseQueryCommandResult(List<string> QueryResult);
}
