// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[HiddenCommand]
[CommandMetadata(
    Id = "ca454f68-3c44-47e3-bd88-6596a1d2c368",
    Name = "write",
    Title = "Write OneLake File",
    Description = "Write content to a file in OneLake storage. Can write text content directly or upload from a local file.",
    Destructive = true,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class FileWriteCommand(
    ILogger<FileWriteCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<FileWriteOptions, FileWriteCommand.FileWriteCommandResult>
{
    private readonly ILogger<FileWriteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(FileWriteOptions options, ValidationResult validationResult)
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

        if (string.IsNullOrWhiteSpace(options.Content) && string.IsNullOrWhiteSpace(options.LocalFilePath))
        {
            validationResult.Errors.Add("Content source is required. Provide --content or --local-file-path.");
        }

        if (!string.IsNullOrWhiteSpace(options.Content) && !string.IsNullOrWhiteSpace(options.LocalFilePath))
        {
            validationResult.Errors.Add("Provide only one content source. Specify either --content or --local-file-path, not both.");
        }

        if (!string.IsNullOrWhiteSpace(options.LocalFilePath) && !System.IO.File.Exists(options.LocalFilePath))
        {
            validationResult.Errors.Add($"Local file not found: {options.LocalFilePath}");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FileWriteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            Stream contentStream;
            long contentLength;

            // Determine content source
            if (!string.IsNullOrEmpty(options.LocalFilePath))
            {
                contentStream = System.IO.File.OpenRead(options.LocalFilePath);
                contentLength = new FileInfo(options.LocalFilePath).Length;
            }
            else
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(options.Content!);
                contentStream = new MemoryStream(bytes);
                contentLength = bytes.Length;
            }

            using (contentStream)
            {
                await _oneLakeService.WriteFileAsync(
                    workspaceIdentifier,
                    itemIdentifier,
                    options.FilePath,
                    contentStream,
                    options.Overwrite,
                    cancellationToken);
            }

            var result = new FileWriteCommandResult(
                options.FilePath,
                contentLength,
                options.Overwrite ? "File written successfully (overwritten)" : "File written successfully");

            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.FileWriteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing file {FilePath} to workspace {WorkspaceId}, item {ItemId}.",
                options.FilePath, options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record FileWriteCommandResult(
        string FilePath,
        long ContentLength,
        string Message);
}

public sealed class FileWriteOptions
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
}
