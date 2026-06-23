// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[CommandMetadata(
    Id = "f6b3249d-6481-4e80-9d34-0d6867718dd7",
    Name = "upload_file",
    Title = "Upload OneLake File",
    Description = "Uploads a file to OneLake storage from inline content or local file path. Use this when the user needs to store data in OneLake. Supports overwrite control and content type specification.",
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class BlobPutCommand(
    ILogger<BlobPutCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<BlobPutOptions, BlobPutCommand.BlobPutCommandResult>
{
    private readonly ILogger<BlobPutCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(BlobPutOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }

        if (string.IsNullOrWhiteSpace(options.ItemId) && string.IsNullOrWhiteSpace(options.Item))
        {
            validationResult.Errors.Add("Item identifier is required. Provide --item or --item-id.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BlobPutOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            using var contentStream = ResolveContentStream(options, out var contentLength);

            var result = await _oneLakeService.PutBlobAsync(
                workspaceIdentifier,
                itemIdentifier,
                options.FilePath,
                contentStream,
                contentLength,
                options.ContentType,
                options.Overwrite,
                cancellationToken);

            var commandResult = new BlobPutCommandResult(
                result.WorkspaceId,
                result.ItemId,
                result.Path,
                result.ContentLength,
                result.ContentType,
                result.ETag,
                result.LastModified,
                result.RequestId,
                result.Version,
                result.RequestServerEncrypted,
                result.ContentMd5,
                result.ContentCrc64,
                result.EncryptionScope,
                result.EncryptionKeySha256,
                result.VersionId,
                result.ClientRequestId,
                result.RootActivityId,
                options.Overwrite ? "File uploaded successfully (overwritten)." : "File uploaded successfully.");

            context.Response.Status = HttpStatusCode.Created;
            context.Response.Results = ResponseResult.Create(commandResult, OneLakeJsonContext.Default.BlobPutCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading blob {BlobPath} in workspace {WorkspaceId}, item {ItemId}.",
                options.FilePath, options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static Stream ResolveContentStream(BlobPutOptions options, out long contentLength)
    {
        if (!string.IsNullOrEmpty(options.LocalFilePath))
        {
            if (!System.IO.File.Exists(options.LocalFilePath))
            {
                throw new FileNotFoundException($"Local file not found: {options.LocalFilePath}");
            }

            var fileStream = System.IO.File.OpenRead(options.LocalFilePath);
            contentLength = fileStream.Length;
            return fileStream;
        }

        if (!string.IsNullOrEmpty(options.Content))
        {
            var bytes = Encoding.UTF8.GetBytes(options.Content);
            contentLength = bytes.LongLength;
            return new MemoryStream(bytes);
        }

        throw new ArgumentException("Either --content or --local-file-path must be specified when uploading a blob.");
    }

    public sealed record BlobPutCommandResult(
        string WorkspaceId,
        string ItemId,
        string BlobPath,
        long ContentLength,
        string ContentType,
        string? ETag,
        DateTimeOffset? LastModified,
        string? RequestId,
        string? Version,
        bool? RequestServerEncrypted,
        string? ContentMd5,
        string? ContentCrc64,
        string? EncryptionScope,
        string? EncryptionKeySha256,
        string? VersionId,
        string? ClientRequestId,
        string? RootActivityId,
        string Message);
}

public sealed class BlobPutOptions
{
    [Option(Description = "The ID of the Microsoft Fabric workspace.")]
    public string? WorkspaceId { get; set; }

    [Option(Description = "The name or ID of the Microsoft Fabric workspace.")]
    public string? Workspace { get; set; }

    [Option(Description = "The ID of the Fabric item.")]
    public string? ItemId { get; set; }

    [Option(Description = "The name or ID of the Fabric item. When using friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse', 'ItemName.Warehouse').")]
    public string? Item { get; set; }

    [Option(Description = "The path to the file in OneLake.")]
    public required string FilePath { get; set; }

    [Option(Description = "The content to write to the file.")]
    public string? Content { get; set; }

    [Option(Description = "The path to a local file to upload.")]
    public string? LocalFilePath { get; set; }

    [Option(Description = "Whether to overwrite existing files.")]
    public bool Overwrite { get; set; }

    [Option(Description = "MIME content type to set on the uploaded file (e.g., 'application/json'). Defaults to 'application/octet-stream'.")]
    public string? ContentType { get; set; }
}
