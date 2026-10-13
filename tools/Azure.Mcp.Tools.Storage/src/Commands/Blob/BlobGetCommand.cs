// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Storage.Commands.Blob;

[CommandMetadata(
    Id = "d6bdc190-e68f-49af-82e7-9cf6ec9b8183",
    Name = "get",
    Title = "Get Storage Blob Details",
    Description = """
        List/get/show blobs in a blob container in Storage account. Use this tool to list the blobs in a container or
        get details for a specific blob. If no blob specified, lists all blobs present in the container, optionally
        filtering on a prefix. The prefix is ignored if a blob is specified.

        Required: --account, --container
        Optional: --blob, --tenant, --prefix

        Returns: blob name, size, lastModified, contentType, contentHash, metadata, and blob properties.
        Do not use this tool to list containers in the storage account.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class BlobGetCommand(ILogger<BlobGetCommand> logger, IStorageService storageService)
    : AuthenticatedCommand<BlobGetOptions, BlobGetCommand.BlobGetCommandResult>
{
    private readonly ILogger<BlobGetCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BlobGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var details = await _storageService.GetBlobDetails(
                options.Account,
                options.Container,
                options.Blob,
                options.Prefix,
                options.Tenant,
                cancellationToken
            );

            context.Response.Results = ResponseResult.Create(new BlobGetCommandResult(details ?? []), StorageJsonContext.Default.BlobGetCommandResult);
            return context.Response;
        }
        catch (Exception ex)
        {
            if (options.Blob is null)
            {
                _logger.LogError(ex, "Error listing blob details. Account: {Account}, Container: {Container}.", options.Account, options.Container);
            }
            else
            {
                _logger.LogError(ex, "Error getting blob details. Account: {Account}, Container: {Container}, Blob: {Blob}.", options.Account, options.Container, options.Blob);
            }
            HandleException(context, ex);
            return context.Response;
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden && reqEx.ErrorCode is "AuthorizationPermissionMismatch" or "InsufficientAccountPermissions" =>
            $"Access denied reading blob details. This commonly happens when the caller has a management-plane role (e.g., Contributor/Owner) but lacks a data-plane role such as 'Storage Blob Data Reader' or 'Storage Blob Data Contributor' on this storage account. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Access denied reading blob details. This can result from a missing data-plane RBAC role, storage account network/firewall restrictions, or an invalid/expired credential. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.ErrorCode == "BlobNotFound" =>
            $"Blob not found. Verify the blob name exists in the specified container. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.ErrorCode == "ContainerNotFound" =>
            $"Container not found. Verify the container exists in the specified storage account. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            $"Container or blob not found. Verify the account, container, and blob names. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public record BlobGetCommandResult(List<BlobInfo> Blobs);
}
