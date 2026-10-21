// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
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
        get details for a specific blob. Use --prefix to filter blob names and --delimiter '/' to browse virtual
        directories one level at a time. Use --max-results to limit the combined number of blobs and directory
        prefixes returned per page, and --continuation-token to retrieve the next page with the same listing options.
        Listing is paged when --delimiter, --max-results, or --continuation-token is supplied, with a default page size of 100 and a
        maximum of 5000. Otherwise, lists all matching blobs as before. The prefix is ignored if a blob is specified.
        The delimiter, max-results, and continuation-token options cannot be used with --blob.

        Required: --account, --container
        Optional: --blob, --tenant, --prefix, --delimiter, --max-results, --continuation-token

        Returns: blob name, size, lastModified, contentType, contentHash, metadata, and blob properties.
        Paged listings also return prefixes (virtual directory paths) and a continuationToken when more results exist.
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

    public override void ValidateOptions(BlobGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.MaxResults is < 1 or > 5000)
        {
            validationResult.Errors.Add("--max-results must be between 1 and 5000.");
        }

        if (options.Delimiter is { Length: 0 })
        {
            validationResult.Errors.Add("--delimiter must not be empty.");
        }

        if (!string.IsNullOrEmpty(options.Blob) && UsesPagedListing(options))
        {
            validationResult.Errors.Add("--delimiter, --max-results, and --continuation-token cannot be used with --blob.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BlobGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            if (UsesPagedListing(options))
            {
                var page = await _storageService.ListBlobs(
                    options.Account,
                    options.Container,
                    options.MaxResults ?? 100,
                    options.Prefix,
                    options.Delimiter,
                    options.ContinuationToken,
                    options.Tenant,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new BlobGetCommandResult(page.Blobs, page.Prefixes, page.ContinuationToken),
                    StorageJsonContext.Default.BlobGetCommandResult);
                return context.Response;
            }

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

    private static bool UsesPagedListing(BlobGetOptions options) =>
        options.Delimiter is not null || options.MaxResults.HasValue || options.ContinuationToken is not null;

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

    public record BlobGetCommandResult(
        List<BlobInfo> Blobs,
        List<string>? Prefixes = null,
        string? ContinuationToken = null);
}
