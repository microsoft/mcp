// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger)
    : BaseSqlCommand<DatabaseListOptions>(logger)
{
    private const string CommandTitle = "List SQL Databases";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all databases in an Azure SQL Server with their configuration, status, SKU, and performance details.
        Use when you need to: view database inventory, check database status across a server, compare database configurations,
        or find databases for management operations.
        Requires: subscription ID, resource group name, server name.
        Returns: JSON array of databases with complete configuration details including SKU, status, and size information.
        Equivalent to 'az sql db list'.
        """;

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var sqlService = context.GetService<ISqlService>();

            var databases = await sqlService.ListDatabasesAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(databases ?? []), SqlJsonContext.Default.DatabaseListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL databases. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record DatabaseListResult(List<SqlDatabase> Databases);
}
