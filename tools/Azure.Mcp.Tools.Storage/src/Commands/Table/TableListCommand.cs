// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Table.Commands;

public sealed class TableListCommand(ILogger<TableListCommand> logger) : BaseStorageCommand<BaseStorageOptions>()
{
    private const string CommandTitle = "List Tables in Azure Storage";
    private readonly ILogger<TableListCommand> _logger = logger;

    public override string Id => "1236ad1d-baf1-4b95-8c1d-420637ce08da";

    public override string Name => "list";

    public override string Description => "List all tables in an Azure Storage account.";

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var tablesService = context.GetService<IStorageService>();
            var tables = await tablesService.ListTables(
                options.Account!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(tables ?? []), StorageJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. StorageAccount: {StorageAccount}.", options.Account);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TableListCommandResult(List<string> Tables);
}
