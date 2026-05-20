// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
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
        filtering on a prefix. The prefix is ignored if a blob is specified. The results may also be filtered by
        selecting which fields to include in the output. If no fields are selected, all fields will be included.
        The details returned include blob name, size, last modified time, content type, content hash, metadata, and
        blob properties. This tool is not intended to list containers in the storage account; use the appropriate
        command for that operation.
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

            if (!string.IsNullOrEmpty(options.SelectedFields))
            {
                var dynamicOptions = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                    TypeInfoResolver = new CommandResponseSelectProperties(options.SelectedFields.Split(','), StorageJsonContext.Default, typeof(BlobInfo))
                };
                context.Response.Results = ResponseResult.Create(new BlobGetCommandResult(details ?? []), dynamicOptions.GetTypeInfo(typeof(BlobGetCommandResult)));
            }
            else
            {
                context.Response.Results = ResponseResult.Create(new(details ?? []), StorageJsonContext.Default.BlobGetCommandResult);
            }

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
