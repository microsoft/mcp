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
    Id = "16f02fbf-6760-440a-bacc-925365b6de49",
    Name = "update",
    Title = "Update SQL Database",
    Description = """
        Scale and configure Azure SQL Database performance settings.
        Update an existing database's SKU, compute tier, storage capacity,
        or redundancy options to meet changing performance requirements.
        Returns the updated database configuration including applied scaling changes.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DatabaseUpdateCommand(ISqlService sqlService, ILogger<DatabaseUpdateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<DatabaseUpdateOptions, DatabaseUpdateCommand.DatabaseUpdateResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<DatabaseUpdateCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DatabaseUpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var database = await _sqlService.UpdateDatabaseAsync(
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

            context.Response.Results = ResponseResult.Create(new(database), SqlJsonContext.Default.DatabaseUpdateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.Database, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL database or server not found. Verify the database name, server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed updating the SQL database. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Invalid database configuration. Check your update parameters. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DatabaseUpdateResult(SqlDatabase Database);
}
