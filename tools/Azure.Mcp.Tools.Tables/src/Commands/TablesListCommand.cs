// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Tables.Options;
using Azure.Mcp.Tools.Tables.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Tables.Commands;

public sealed class TablesListCommand(ILogger<TablesListCommand> logger) : BaseTablesCommand<TablesListOptions>()
{
    private const string CommandTitle = "List Azure Table storage tables";
    private readonly ILogger<TablesListCommand> _logger = logger;

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
            var tablesService = context.GetService<ITablesService>();
            var tables = await tablesService.ListTables(
                options.Account!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(tables ?? []), TablesJsonContext.Default.TablesListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. Account: {Account}.", options.Account);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TablesListCommandResult(List<string> Tables);
}
