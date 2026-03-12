// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.Database;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Sql.Commands.Database;

public sealed class DatabaseGetCommand(ILogger<DatabaseGetCommand> logger)
    : BaseSqlCommand<DatabaseGetOptions>(logger)
{
    private const string CommandTitle = "Get SQL Database";

    public override string Id => "2c4e6a8b-1d3f-4e5a-b6c7-8d9e0f1a2b3c";

    public override string Name => "get";

    public override string Description =>
        """
        Show, get, or list Azure SQL databases in a SQL Server. Shows details for a specific Azure SQL database
        by name, or lists all Azure SQL databases in the specified SQL Server. Use to show or retrieve Azure SQL
        database information. Equivalent to 'az sql db show' (show one Azure SQL database) or 'az sql db list'
        (list all Azure SQL databases in a server). Returns database information including configuration details
        and current status.
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.Database.AsOptional());
    }

    protected override DatabaseGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Database = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.Database.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var sqlService = context.GetService<ISqlService>();

            if (!string.IsNullOrEmpty(options.Database))
            {
                var database = await sqlService.GetDatabaseAsync(
                    options.Server!,
                    options.Database,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new([database], false),
                    SqlJsonContext.Default.DatabaseGetListResult);
            }
            else
            {
                var result = await sqlService.ListDatabasesAsync(
                    options.Server!,
                    options.ResourceGroup!,
                    options.Subscription!,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new(result?.Results ?? [], result?.AreResultsTruncated ?? false),
                    SqlJsonContext.Default.DatabaseGetListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL database(s). Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.Database, options.ResourceGroup, options);
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

    internal record DatabaseGetListResult(List<SqlDatabase> Databases, bool AreResultsTruncated);
}
