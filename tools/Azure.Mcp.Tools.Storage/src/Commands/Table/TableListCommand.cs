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
    Description = "List all tables in an Azure Storage account. Shows table names for the specified storage account. Required: account. Optional: tenant. Returns: table names. Do not use this tool for Cosmos DB tables or Kusto/Data Explorer tables.",
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false,
    CustomCloudRequirement = CustomCloudRequirement.Unsupported)]
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
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new TableListCommandResult(tables ?? []), StorageJsonContext.Default.TableListCommandResult);
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

    public record TableListCommandResult(List<string> Tables);
}
