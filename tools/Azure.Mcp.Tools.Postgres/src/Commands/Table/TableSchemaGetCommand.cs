// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Options.Table;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Postgres.Commands.Table;

[CommandMetadata(
    Id = "643a3497-44e1-4727-b3d6-c2e5dba6cab2",
    Name = "get",
    Title = "Get PostgreSQL Table Schema",
    Description = "Retrieves the schema of a specified table in a PostgreSQL database.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableSchemaGetCommand(IPostgresService postgresService, ILogger<TableSchemaGetCommand> logger) : BaseDatabaseCommand<TableSchemaGetOptions>(logger)
{
    private readonly IPostgresService _postgresService = postgresService;

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {

            List<string> schema = await _postgresService.GetTableSchemaAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.AuthType!,
                options.User!,
                options.Password,
                options.Server!,
                options.Database!,
                options.Table!,
                cancellationToken);
            context.Response.Results = ResponseResult.Create(new(schema ?? []), PostgresJsonContext.Default.TableSchemaGetCommandResult);
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
