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
    Id = "3d7ce5ba-e365-4e5c-9542-c2550c0fd11a",
    Name = "list",
    Title = "List OneLake Blobs",
    Description = "List files and directories in OneLake storage as blobs. Browse the contents of a lakehouse or specific directory path with optional recursive listing in blob format. If no path is specified, intelligently discovers content by searching both Files and Tables folders automatically, providing comprehensive visibility across all top-level OneLake folders. Use --format=raw to get the unprocessed OneLake API response for debugging.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class BlobListCommand(
    ILogger<BlobListCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<BlobListOptions, BlobListCommand.BlobListCommandResult>
{
    private readonly ILogger<BlobListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(BlobListOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, BlobListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace!;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item!;

            // Check if raw format is requested
            if (options.Format?.ToLowerInvariant() == "raw")
            {
                var rawResponse = await _oneLakeService.ListBlobsRawAsync(
                    workspaceIdentifier,
                    itemIdentifier,
                    options.Path,
                    options.Recursive,
                    cancellationToken);

                var rawResult = new BlobListCommandResult { RawResponse = rawResponse };
                context.Response.Results = ResponseResult.Create(rawResult, MinimalJsonContext.Default.BlobListCommandResult);
                return context.Response;
            }

            List<OneLakeFileInfo> files;

            // Use intelligent discovery if no path is specified
            if (string.IsNullOrWhiteSpace(options.Path))
            {
                files = (await _oneLakeService.ListBlobsIntelligentAsync(
                    workspaceIdentifier,
                    itemIdentifier,
                    options.Recursive,
                    cancellationToken)).ToList();
            }
            else
            {
                files = (await _oneLakeService.ListBlobsAsync(
                    workspaceIdentifier,
                    itemIdentifier,
                    options.Path,
                    options.Recursive,
                    cancellationToken)).ToList();
            }

            var result = new BlobListCommandResult(files, options.Path ?? "");
            context.Response.Results = ResponseResult.Create(result, MinimalJsonContext.Default.BlobListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs in workspace {WorkspaceId}, item {ItemId}, path {Path}.",
                options.WorkspaceId, options.ItemId, options.Path);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record BlobListCommandResult
    {
        public List<OneLakeFileInfo>? Files { get; init; }
        public string? BasePath { get; init; }
        public string? RawResponse { get; init; }

        public BlobListCommandResult(List<OneLakeFileInfo> files, string basePath)
        {
            Files = files;
            BasePath = basePath;
        }

        public BlobListCommandResult()
        {
        }
    }
}
