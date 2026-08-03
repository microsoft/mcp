// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

[CommandMetadata(
    Id = "2c4e6a8b-1d3f-4e5a-b6c7-8d9e0f1a2b3c",
    Name = "get",
    Title = "Get SQL Database",
    Description = """
        Show, get, or list Azure SQL databases in a SQL Server. Shows details for a specific Azure SQL database
        by name, or lists all Azure SQL databases in the specified SQL Server. Equivalent to 'az sql db show'
        (show one Azure SQL database) or 'az sql db list' (list all Azure SQL databases in a server).
        Returns database information including configuration details and current status.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseGetCommand(ISqlService sqlService, ILogger<DatabaseGetCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DatabaseGetOptions, DatabaseGetCommand.DatabaseGetListResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<DatabaseGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(options.Database))
            {
                var database = await _sqlService.GetDatabaseAsync(
                    options.Server,
                    options.Database,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([database]),
                    SqlJsonContext.Default.DatabaseGetListResult);
            }
            else
            {
                var databases = await _sqlService.ListDatabasesAsync(
                    options.Server,
                    options.ResourceGroup,
                    options.Subscription!,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(databases ?? []),
                    SqlJsonContext.Default.DatabaseGetListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL database(s). Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.Database, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL database or server not found. Verify the server name, database name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the SQL database. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DatabaseGetListResult(List<SqlDatabase> Databases);
}
