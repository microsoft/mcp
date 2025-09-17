// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

public sealed class DatabaseDeleteCommand(ILogger<DatabaseDeleteCommand> logger)
    : BaseDatabaseCommand<DatabaseDeleteOptions>(logger)
{
    private const string CommandTitle = "Delete SQL Database";

    public override string Name => "delete";

    public override string Description =>
        """
		Deletes an Azure SQL Database from an existing SQL Server. This command removes the specified database
		and is idempotent - attempting to delete a database that does not exist will succeed with Deleted = false.
		Equivalent to 'az sql db delete'.
		Returns whether the database was deleted during this operation.
		""";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
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

            var deleted = await sqlService.DeleteDatabaseAsync(
                options.Server!,
                options.Database!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new DatabaseDeleteResult(deleted, options.Database!),
                SqlJsonContext.Default.DatabaseDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.Database, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server or database not found. Verify the server name, database name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed deleting the SQL database. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record DatabaseDeleteResult(bool Deleted, string DatabaseName);
}

