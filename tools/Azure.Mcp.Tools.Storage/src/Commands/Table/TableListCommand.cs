// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Table.Commands;

public sealed class TableListCommand(ILogger<TableListCommand> logger, IStorageService storageService) : BaseStorageCommand<BaseStorageOptions, TableListCommand.TableListCommandResult>()
{
    private const string CommandTitle = "List Tables in Azure Storage";
    private readonly ILogger<TableListCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override string Id => "1236ad1d-baf1-4b95-8c1d-420637ce08da";

    public override string Name => "list";

    public override string Description => "List all tables in an Azure Storage account. Shows table names for the specified storage account. Required: account, subscription. Optional: tenant. Returns: table names. Do not use this tool for Cosmos DB tables or Kusto/Data Explorer tables.";

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

    protected override JsonTypeInfo<TableListCommandResult> ResultTypeInfo => StorageJsonContext.Default.TableListCommandResult;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var tables = await _storageService.ListTables(
                options.Account!,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            SetResult(context, new(tables ?? []));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. StorageAccount: {StorageAccount}.", options.Account);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record TableListCommandResult(List<string> Tables);
}
