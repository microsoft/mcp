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
    Id = "a4d9af17-fe8b-4df3-93be-23b69f0b5a0c",
    Name = "create",
    Title = "Create SQL Database",
    Description = """
        Create a new Azure SQL Database on an existing SQL Server with configurable performance tiers, size limits,
        and other settings. Equivalent to 'az sql db create'.
        Returns the newly created database information including configuration details.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseCreateCommand(ISqlService sqlService, ILogger<DatabaseCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DatabaseCreateOptions, DatabaseCreateCommand.DatabaseCreateResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<DatabaseCreateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var database = await _sqlService.CreateDatabaseAsync(
                options.Server,
                options.Database,
                options.ResourceGroup,
                options.Subscription!,
                options.SkuName,
                options.SkuTier,
                options.SkuCapacity,
                options.Collation,
                options.MaxSizeBytes,
                options.ElasticPoolName,
                options.ZoneRedundant,
                options.ReadScale,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(database), SqlJsonContext.Default.DatabaseCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.Database, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Database already exists with the specified name. Choose a different database name or use the update command.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the SQL database. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Invalid database configuration. Check your SKU, size, and other parameters. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DatabaseCreateResult(SqlDatabase Database);
}
