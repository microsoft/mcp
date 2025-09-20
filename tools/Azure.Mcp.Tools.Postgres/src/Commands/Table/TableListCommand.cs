// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
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

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
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
            IPostgresService pgService = context.GetService<IPostgresService>() ?? throw new InvalidOperationException("PostgreSQL service is not available.");
            List<string> tables = await pgService.ListTablesAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Database!);
            context.Response.Results = ResponseResult.Create(new(tables ?? []), PostgresJsonContext.Default.TableListCommandResult);
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
