// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

public sealed class BlobPutCommand(
    ILogger<BlobPutCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<BlobPutOptions>()
{
    private readonly ILogger<BlobPutCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "f6b3249d-6481-4e80-9d34-0d6867718dd7";
    public override string Name => "upload-file";
    public override string Title => "Upload OneLake File";
    public override string Description => "Uploads a file to OneLake storage from inline content or local file path. Use this when the user needs to store data in OneLake. Supports overwrite control and content type specification.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        LocalRequired = true,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.FilePath);
        command.Options.Add(FabricOptionDefinitions.Content);
        command.Options.Add(FabricOptionDefinitions.LocalFilePath);
        command.Options.Add(FabricOptionDefinitions.Overwrite);
        command.Options.Add(FabricOptionDefinitions.ContentType);
        command.Validators.Add(result =>
        {
            var localFilePath = result.GetValueOrDefault<string>(FabricOptionDefinitions.LocalFilePath.Name);
            var content = result.GetValueOrDefault<string>(FabricOptionDefinitions.Content.Name);
            if (string.IsNullOrEmpty(localFilePath) && string.IsNullOrEmpty(content))
            {
                result.AddError("Either --content or --local-file-path must be specified.");
            }
            else if (!string.IsNullOrEmpty(localFilePath) && !string.IsNullOrEmpty(content))
            {
                result.AddError("Only one of --content or --local-file-path can be specified, not both.");
            }

            if (!string.IsNullOrEmpty(localFilePath))
            {
                if (!System.IO.File.Exists(localFilePath))
                {
                    result.AddError($"Local file not found: {localFilePath}");
                }
            }
        });
    }

    protected override BlobPutOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FilePath.Name) ?? string.Empty;
        options.Content = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Content.Name);
        options.LocalFilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.LocalFilePath.Name);
        options.Overwrite = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Overwrite.Name);
        options.ContentType = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContentType.Name);
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
            using var contentStream = ResolveContentStream(options, out var contentLength);

            var result = await _oneLakeService.PutBlobAsync(
                options.WorkspaceId!,
                options.ItemId!,
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
            _logger.LogError(ex, "Error uploading blob {BlobPath} in workspace {WorkspaceId}, item {ItemId}. Options: {@Options}",
                options.FilePath, options.WorkspaceId, options.ItemId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static Stream ResolveContentStream(BlobPutOptions options, out long contentLength)
    {
        if (!string.IsNullOrEmpty(options.LocalFilePath))
        {
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

public sealed class BlobPutOptions : BaseItemOptions
{
    public string FilePath { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? LocalFilePath { get; set; }
    public bool Overwrite { get; set; }
    public string? ContentType { get; set; }
}
