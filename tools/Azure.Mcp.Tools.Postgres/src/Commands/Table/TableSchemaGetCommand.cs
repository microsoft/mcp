// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Options.Table;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands.Table;

public sealed class TableSchemaGetCommand(ILogger<TableSchemaGetCommand> logger) : BaseDatabaseCommand<TableSchemaGetOptions>(logger)
{
    private const string CommandTitle = "Get PostgreSQL Table Schema";

    public override string Name => "schema";
    public override string Description => "Retrieves the schema of a specified table in a PostgreSQL database.";
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
        command.Options.Add(PostgresOptionDefinitions.Table);
    }

    protected override TableSchemaGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Table = parseResult.GetValueOrDefault<string>(PostgresOptionDefinitions.Table.Name);
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
