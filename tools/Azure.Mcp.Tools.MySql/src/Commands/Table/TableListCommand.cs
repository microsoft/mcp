// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Json;
using Azure.Mcp.Tools.MySql.Options.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands.Table;

public sealed class TableListCommand(ILogger<TableListCommand> logger) : BaseDatabaseCommand<TableListOptions>(logger)
{
    private const string CommandTitle = "List MySQL Tables";

    public override string Name => "list";

    public override string Description => "Enumerates all tables within a specified database on an Azure Database for MySQL Flexible Server instance. This command provides a complete inventory of table objects, facilitating database exploration, schema analysis, and data architecture understanding for development tasks.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            IMySqlService mysqlService = context.GetService<IMySqlService>() ?? throw new InvalidOperationException("MySQL service is not available.");
            List<string> tables = await mysqlService.GetTablesAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Database!);
            context.Response.Results = tables?.Count > 0 ?
                ResponseResult.Create(
                    new TableListCommandResult(tables),
                    MySqlJsonContext.Default.TableListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing tables.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record TableListCommandResult(List<string> Tables);
}
