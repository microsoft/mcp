// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Pagination;
using Azure.Mcp.Tools.Storage.Commands.Blob.Container;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;
using Microsoft.Mcp.Core.Services.Pagination;

namespace Azure.Mcp.Tools.Storage.Commands.Blob;

public sealed class BlobGetCommand : BaseContainerCommand<BlobGetOptions>
{
    private const string CommandTitle = "Get Storage Blob Details";
    private readonly ILogger<BlobGetCommand> _logger;
    private readonly IStorageService _storageService;
    private readonly IPaginationService? _paginationService;

    public BlobGetCommand(ILogger<BlobGetCommand> logger, IStorageService storageService)
        : this(logger, storageService, null)
    {
    }

    public BlobGetCommand(ILogger<BlobGetCommand> logger, IStorageService storageService, IPaginationService? paginationService)
    {
        _logger = logger;
        _storageService = storageService;
        _paginationService = paginationService;
    }

    public override string Id => "d6bdc190-e68f-49af-82e7-9cf6ec9b8183";

    public override string Name => "get";

    public override string Description =>
        $"""
        List/get/show blobs in a blob container in Storage account. Use this tool to list the blobs in a container or get details for a specific blob. Shows blob properties including metadata, size, last modification time, and content properties. If no blob specified, lists all blobs present in the container. Required: account, container <container>, subscription <subscription>. Optional: blob <blob>, tenant <tenant>. Returns: blob name, size, lastModified, contentType, contentMD5, metadata, and blob properties. Do not use this tool to list containers in the storage account.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false,
        SupportsPagination = _paginationService is not null
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Blob.AsOptional());
        command.Options.Add(OptionDefinitions.Pagination.Cursor.AsOptional());
    }

    protected override BlobGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Blob = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Blob.Name);
        options.Cursor = parseResult.GetValueOrDefault<string>(OptionDefinitions.Pagination.Cursor.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            if (_paginationService is not null && string.IsNullOrEmpty(options.Blob))
            {
                return await ExecutePagedAsync(context, options, cancellationToken);
            }

            var details = await _storageService.GetBlobDetails(
                options.Account!,
                options.Container!,
                options.Blob,
                options.Subscription!,
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

    private async Task<CommandResponse> ExecutePagedAsync(CommandContext context, BlobGetOptions options, CancellationToken cancellationToken)
    {
        var operationName = nameof(BlobGetCommand);
        var fingerprintParams = new Dictionary<string, string?>
        {
            ["operation"] = operationName,
            ["subscription"] = options.Subscription,
            ["account"] = options.Account,
            ["container"] = options.Container,
        };
        var fingerprint = _paginationService!.ComputeRequestFingerprint(fingerprintParams);

        var adapter = new KqlPaginationAdapter<BlobInfo>(
            continuationToken => _storageService.GetBlobDetailsPaged(
                options.Account!, options.Container!, options.Subscription!,
                options.Tenant, options.RetryPolicy, continuationToken, cancellationToken));

        PageResult<BlobInfo>? pagedResult;
        if (string.IsNullOrEmpty(options.Cursor))
        {
            pagedResult = await adapter.FetchFirstPageAsync(cancellationToken);
        }
        else
        {
            var cursorRecord = await _paginationService.LoadAndValidateCursorAsync(
                options.Cursor!, fingerprint, cancellationToken);
            pagedResult = await adapter.FetchNextPageAsync(cursorRecord.NativeState, cancellationToken);
        }

        string? nextCursor = null;
        if (pagedResult.NativeState is not null)
        {
            nextCursor = await _paginationService.SaveCursorAsync(
                KqlPaginationAdapter<BlobInfo>.ProviderName, operationName,
                fingerprint, pagedResult.NativeState,
                cancellationToken: cancellationToken);
        }

        context.Response.Results = ResponseResult.Create(
            new BlobGetCommandResult(pagedResult.Items, nextCursor),
            StorageJsonContext.Default.BlobGetCommandResult);

        return context.Response;
    }

    internal record BlobGetCommandResult(List<BlobInfo> Blobs, string? NextCursor = null);
}
