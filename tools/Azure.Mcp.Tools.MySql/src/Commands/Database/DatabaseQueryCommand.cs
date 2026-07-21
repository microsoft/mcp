// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.MySql.Options.Database;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.MySql.Commands.Database;

[CommandMetadata(
    Id = "b73afaa5-4c3f-41e8-9ef3-c54e75215a97",
    Name = "query",
    Title = "Query MySQL Database",
    Description = "Executes a safe, read-only SQL SELECT query against a database on Azure Database for MySQL Flexible Server. Use this tool to explore or retrieve table data without modifying it. Rejects non-SELECT statements (INSERT/UPDATE/DELETE/REPLACE/MERGE/TRUNCATE/ALTER/CREATE/DROP), multi-statements, comments hiding writes, transaction control (BEGIN/COMMIT/ROLLBACK), INTO OUTFILE, and other destructive keywords. Only a single SELECT is executed to ensure data integrity. Best practices: List needed columns (avoid SELECT *), add WHERE filters, use LIMIT/OFFSET for paging, ORDER BY for deterministic results, and avoid unnecessary sensitive data. Example: SELECT id, name, status FROM customers WHERE status = 'Active' ORDER BY name LIMIT 50;",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseQueryCommand(ILogger<DatabaseQueryCommand> logger, IMySqlService mysqlService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DatabaseQueryOptions, DatabaseQueryCommand.DatabaseQueryCommandResult>(subscriptionResolver)
{
    private readonly IMySqlService _mysqlService = mysqlService;
    private readonly ILogger<DatabaseQueryCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseQueryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mysqlService.ExecuteQueryAsync(options.Subscription!, options.ResourceGroup, options.User, options.Server, options.Database, options.Query, cancellationToken);
            context.Response.Results = ResponseResult.Create(new(result ?? []), MySqlJsonContext.Default.DatabaseQueryCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred executing query.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public sealed record DatabaseQueryCommandResult(List<string> Results);
}
