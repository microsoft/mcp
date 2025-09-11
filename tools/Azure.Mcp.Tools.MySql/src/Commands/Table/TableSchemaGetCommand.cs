// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Json;
using Azure.Mcp.Tools.MySql.Options;
using Azure.Mcp.Tools.MySql.Options.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.MySql.Commands.Table;

public sealed class TableSchemaGetCommand(ILogger<TableSchemaGetCommand> logger) : BaseDatabaseCommand<TableSchemaGetOptions>(logger)
{
    private const string CommandTitle = "Get MySQL Table Schema";
    private readonly Option<string> _tableOption = MySqlOptionDefinitions.Table;

    public override string Name => "schema";

    public override string Description => "Retrieves detailed schema information for a specific table within an Azure Database for MySQL Flexible Server database. This command provides comprehensive metadata including column definitions, data types, constraints, indexes, and relationships, essential for understanding table structure and supporting application development.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_tableOption);
    }

    protected override TableSchemaGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Table = parseResult.GetValue(_tableOption);
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
            IMySqlService mysqlService = context.GetService<IMySqlService>() ?? throw new InvalidOperationException("MySQL service is not available.");
            List<string> schema = await mysqlService.GetTableSchemaAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Database!, options.Table!);
            context.Response.Results = schema?.Count > 0 ?
                ResponseResult.Create(
                    new TableSchemaGetCommandResult(schema),
                    MySqlJsonContext.Default.TableSchemaGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving table schema.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record TableSchemaGetCommandResult(List<string> Schema);
}
