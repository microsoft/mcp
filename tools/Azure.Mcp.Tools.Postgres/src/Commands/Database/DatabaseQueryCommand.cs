// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Options.Database;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Postgres.Commands.Database;

public sealed class DatabaseQueryCommand(ILogger<DatabaseQueryCommand> logger) : BaseDatabaseCommand<DatabaseQueryOptions>(logger)
{
    private const string CommandTitle = "Query PostgreSQL Database";
    private readonly Option<string> _queryOption = PostgresOptionDefinitions.Query;
    public override string Name => "query";

    public override string Description => "Executes a query on the PostgreSQL database.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_queryOption);
    }

    protected override DatabaseQueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValue(_queryOption);
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
            List<string> queryResult = await pgService.ExecuteQueryAsync(options.Subscription!, options.ResourceGroup!, options.User!, options.Server!, options.Database!, options.Query!);
            context.Response.Results = queryResult?.Count > 0 ?
                ResponseResult.Create(
                    new DatabaseQueryCommandResult(queryResult),
                    PostgresJsonContext.Default.DatabaseQueryCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while executing the query.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record DatabaseQueryCommandResult(List<string> QueryResult);
}
