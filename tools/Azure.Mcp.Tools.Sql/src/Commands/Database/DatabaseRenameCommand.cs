// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

public sealed class DatabaseRenameCommand(ILogger<DatabaseRenameCommand> logger)
    : BaseDatabaseCommand<DatabaseRenameOptions>(logger)
{
    private const string CommandTitle = "Rename SQL Database";

    public override string Name => "rename";

    public override string Description =>
        """
        Rename an existing Azure SQL Database to a new name within the same SQL server. This command moves the
        database resource to a new identifier while preserving configuration and data. Equivalent to
        'az sql db rename'. Returns the updated database information using the new name.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.NewDatabaseNameOption);
    }

    protected override DatabaseRenameOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.NewDatabaseName = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.NewDatabaseName);
        return options;
    }

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

            var database = await sqlService.RenameDatabaseAsync(
                options.Server!,
                options.Database!,
                options.NewDatabaseName!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(database), SqlJsonContext.Default.DatabaseRenameResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error renaming SQL database. Server: {Server}, Database: {Database}, NewDatabase: {NewDatabase}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.Database, options.NewDatabaseName, options.ResourceGroup, options);
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

    internal record DatabaseRenameResult(SqlDatabase Database);
}
