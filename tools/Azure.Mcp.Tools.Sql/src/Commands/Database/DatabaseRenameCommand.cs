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
    Id = "3bddfa1a-ab9d-44f0-830a-e56a159e5469",
    Name = "rename",
    Title = "Rename SQL Database",
    Description = """
        Rename an existing Azure SQL Database to a new name within the same SQL server. This command moves the
        database resource to a new identifier while preserving configuration and data. Equivalent to
        'az sql db rename'. Returns the updated database information using the new name.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseRenameCommand(ISqlService sqlService, ILogger<DatabaseRenameCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DatabaseRenameOptions, DatabaseRenameCommand.DatabaseRenameResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<DatabaseRenameCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseRenameOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var database = await _sqlService.RenameDatabaseAsync(
                options.Server,
                options.Database,
                options.NewDatabaseName,
                options.ResourceGroup,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(database), SqlJsonContext.Default.DatabaseRenameResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error renaming SQL database. Server: {Server}, Database: {Database}, NewDatabase: {NewDatabase}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.Database, options.NewDatabaseName, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server or database not found. Verify the server name, database name, resource group, and your access permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            $"Database rename conflict. Ensure the destination database name does not already exist. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed renaming the SQL database. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Invalid database rename operation. Check the provided parameters. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DatabaseRenameResult(SqlDatabase Database);
}
