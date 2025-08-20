// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

public sealed class DatabaseShowCommand(ILogger<DatabaseShowCommand> logger)
    : BaseDatabaseCommand<DatabaseShowOptions>(logger)
{
    private const string CommandTitle = "Show SQL Database Details";

    public override string Name => "show";

    public override string Description =>
        """
        Get the details of an Azure SQL Database. This command retrieves detailed information about a specific database
        including its configuration, status, performance tier, and other properties. Equivalent to 'az sql db show'.
        Returns detailed database information including SKU, status, collation, and size information.
          Required options:
        - subscription: Azure subscription ID
        - resource-group: Resource group name containing the SQL server
        - server: Azure SQL Server name
        - database: Database name to retrieve details for
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var sqlService = context.GetService<ISqlService>();

            var database = await sqlService.GetDatabaseAsync(
                options.Server!,
                options.Database!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            if (database == null)
            {
                context.Response.Status = 404;
                context.Response.Message = $"Database '{options.Database}' not found on server '{options.Server}' in resource group '{options.ResourceGroup}'.";
                return context.Response;
            }

            context.Response.Results = ResponseResult.Create(
                new DatabaseShowResult(database),
                SqlJsonContext.Default.DatabaseShowResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error showing SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.Database, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "Database or server not found. Verify the database name, server name, resource group, and that you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the SQL database. Verify you have appropriate permissions. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record DatabaseShowResult(SqlDatabase Database);
}
