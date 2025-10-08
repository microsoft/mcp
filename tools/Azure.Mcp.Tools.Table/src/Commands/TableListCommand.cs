// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Table.Options;
using Azure.Mcp.Tools.Table.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Table.Commands;

public sealed class TableListCommand(ILogger<TableListCommand> logger) : BaseTableCommand<TableListOptions>()
{
    private const string CommandTitle = "List Tables";
    private readonly ILogger<TableListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description => "List all tables in a Storage account.";

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
            var tableService = context.GetService<ITableService>();
            var tables = await tableService.ListTables(
                options.Account!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(tables ?? []), TableJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. Account: {Account}.", options.Account);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TableListCommandResult(List<string> Tables);
}
