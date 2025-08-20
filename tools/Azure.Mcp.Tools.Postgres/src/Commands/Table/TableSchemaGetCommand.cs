// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Options.Table;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands.Table;

public sealed class TableSchemaGetCommand(ILogger<TableSchemaGetCommand> logger) : BaseDatabaseCommand<TableSchemaGetOptions>(logger)
{
    private const string CommandTitle = "Get PostgreSQL Table Schema";
    private readonly Option<string> _tableOption = PostgresOptionDefinitions.Table;

    public override string Name => "schema";
    public override string Description => "Retrieves the schema of a specified table in a PostgreSQL database.";
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_tableOption);
    }

    protected override TableSchemaGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Table = parseResult.GetValueForOption(_tableOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        try
        {
            var options = BindOptions(parseResult);

            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            IPostgresService pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");
            List<string> schema = await pgService.GetTableSchemaAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Database!, options.Table!);
            context.Response.Results = schema?.Count > 0 ?
                ResponseResult.Create(
                    new TableSchemaGetCommandResult(schema),
                    PostgresJsonContext.Default.TableSchemaGetCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred retrieving the schema for table");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TableSchemaGetCommandResult(List<string> Schema);
}
