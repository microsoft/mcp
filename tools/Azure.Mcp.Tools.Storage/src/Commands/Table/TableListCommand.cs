// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Options.Table;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Table.Commands;

[CommandMetadata(
    Id = "1236ad1d-baf1-4b95-8c1d-420637ce08da",
    Name = "list",
    Title = "List Tables in Azure Storage",
    Description = "List one page of up to 100 table names in an Azure Storage account. The page size is fixed. Pass nextCursor unchanged as --cursor with the same account to retrieve another page, even if the current page is empty. Required: account. Optional: tenant, cursor. Returns: table names and nextCursor when more pages are available. Do not use this tool for Cosmos DB tables or Kusto/Data Explorer tables.",
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableListCommand(ILogger<TableListCommand> logger, IStorageService storageService)
    : AuthenticatedCommand<TableListOptions, TableListCommand.TableListCommandResult>
{
    private readonly ILogger<TableListCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var tables = await _storageService.ListTables(
                options.Account,
                options.Cursor,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new TableListCommandResult(tables.Tables ?? [], tables.NextCursor), StorageJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. StorageAccount: {StorageAccount}.", options.Account);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden && reqEx.ErrorCode is "AuthorizationPermissionMismatch" or "InsufficientAccountPermissions" =>
            $"Access denied listing tables. This commonly happens when the caller has a management-plane role (e.g., Contributor/Owner) but lacks a data-plane role such as 'Storage Table Data Reader' or 'Storage Table Data Contributor' on this storage account. See https://learn.microsoft.com/rest/api/storageservices/table-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Access denied listing tables. This can result from a missing data-plane RBAC role, storage account network/firewall restrictions, or an invalid/expired credential. See https://learn.microsoft.com/rest/api/storageservices/table-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"Storage account not found, or this account's SKU/kind does not support the Table service. Verify the account name. See https://learn.microsoft.com/rest/api/storageservices/table-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record TableListCommandResult(List<string> Tables, string? NextCursor = null);
}
