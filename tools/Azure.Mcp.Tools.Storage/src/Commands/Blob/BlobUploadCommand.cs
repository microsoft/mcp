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
    Id = "aafb82ac-e35a-4800-b362-c642a3ac1e17",
    Name = "upload",
    Title = "Upload Local File to Blob",
    Description = """
        Uploads a local file to an Azure Storage blob, only if the blob does not exist, returning the last modified time,
        ETag, and content hash of the uploaded blob.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class BlobUploadCommand(ILogger<BlobUploadCommand> logger, IStorageService storageService)
    : AuthenticatedCommand<BlobUploadOptions, BlobUploadResult>
{
    private readonly ILogger<BlobUploadCommand> _logger = logger;
    private readonly IStorageService _storageService = storageService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BlobUploadOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _storageService.UploadBlob(
                options.Account,
                options.Container,
                options.Blob,
                options.LocalFilePath,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(result, StorageJsonContext.Default.BlobUploadResult);

            _logger.LogInformation("Successfully uploaded file {LocalFilePath} to blob {Blob} in container {Container}.",
                options.LocalFilePath, options.Blob, options.Container);

            return context.Response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {LocalFilePath} to blob {Blob} in container {Container}.",
                options.LocalFilePath, options.Blob, options.Container);
            HandleException(context, ex);
            return context.Response;
        }
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden && reqEx.ErrorCode is "AuthorizationPermissionMismatch" or "InsufficientAccountPermissions" =>
            $"Access denied uploading blob. This commonly happens when the caller has a management-plane role (e.g., Contributor/Owner) but lacks a data-plane role such as 'Storage Blob Data Contributor' or 'Storage Blob Data Owner' on this storage account. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Access denied uploading blob. This can result from a missing data-plane RBAC role, storage account network/firewall restrictions, or an invalid/expired credential. See https://learn.microsoft.com/rest/api/storageservices/blob-service-error-codes for error code details. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.ErrorCode == "BlobAlreadyExists" =>
            $"Blob already exists. This tool only uploads when the blob does not already exist; delete or rename the existing blob first if you intend to replace it, or use 'storage blob get' to check for an existing blob before uploading. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.ErrorCode == "ContainerNotFound" =>
            $"Container not found. Verify the account and container names. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };
}
