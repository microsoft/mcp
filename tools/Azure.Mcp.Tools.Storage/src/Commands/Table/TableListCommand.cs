// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Table.Commands;

[CommandMetadata(
    Id = "1236ad1d-baf1-4b95-8c1d-420637ce08da",
    Name = "list",
    Title = "List Tables in Azure Storage",
    Description = "List all tables in an Azure Storage account. Shows table names for the specified storage account. Required: account, subscription. Optional: tenant. Returns: table names. Do not use this tool for Cosmos DB tables or Kusto/Data Explorer tables.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableListCommand(ILogger<TableListCommand> logger, IStorageService storageService) : BaseStorageCommand<BaseStorageOptions>()
{
    private readonly ILogger<TableListCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

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
