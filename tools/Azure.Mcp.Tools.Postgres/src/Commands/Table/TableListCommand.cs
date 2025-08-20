// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Postgres.Options.Table;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;


namespace Azure.Mcp.Tools.Postgres.Commands.Table;

public sealed class TableListCommand(ILogger<TableListCommand> logger) : BaseDatabaseCommand<TableListOptions>(logger)
{
    private const string CommandTitle = "List PostgreSQL Tables";

    public override string Name => "list";
    public override string Description => "Lists all tables in the PostgreSQL database.";
    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

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
            List<string> tables = await pgService.ListTablesAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Database!);
            context.Response.Results = tables?.Count > 0 ?
                ResponseResult.Create(
                    new TableListCommandResult(tables),
                    PostgresJsonContext.Default.TableListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing tables.");
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record TableListCommandResult(List<string> Tables);
}
