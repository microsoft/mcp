// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
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

        Required: --account, --container, --subscription
        Optional: --blob, --tenant, --prefix

        Returns: blob name, size, lastModified, contentType, contentHash, metadata, and blob properties.
        Do not use this tool to list containers in the storage account.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class BlobGetCommand(ILogger<BlobGetCommand> logger, IStorageService storageService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<BlobGetOptions, BlobGetCommand.BlobGetCommandResult>(subscriptionResolver)
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
                options.Subscription!,
                options.Prefix,
                options.Tenant,
                options.RetryPolicy,
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

    public record BlobGetCommandResult(List<BlobInfo> Blobs);
}
