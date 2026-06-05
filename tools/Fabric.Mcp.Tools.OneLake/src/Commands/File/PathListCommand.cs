// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

[CommandMetadata(
    Id = "3bf1b82d-ff44-4984-9b97-0e6d9e4917a3",
    Name = "list_files",
    Title = "List OneLake Path Structure",
    Description = """
        List files and directories in OneLake storage using a filesystem-style hierarchical view, similar to Azure Data Lake Storage Gen2.
        Shows directory structure with paths, sizes, timestamps, and metadata. Use this to explore OneLake content in a filesystem format
        rather than flat blob listing. Supports optional path filtering and recursive directory traversal.

        If no path is specified, intelligently discovers content by searching both Files and Tables folders automatically,
        providing comprehensive visibility across all top-level OneLake folders.

        Use --format=raw to get the unprocessed OneLake DFS API response for debugging and analysis.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class PathListCommand(IOneLakeService service, ILogger<PathListCommand> logger)
    : AuthenticatedCommand<PathListOptions, PathListCommand.PathListResult>
{
    private readonly ILogger<PathListCommand> _logger = logger;
    private readonly IOneLakeService _service = service;

    public override void ValidateOptions(PathListOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, PathListOptions options, CancellationToken cancellationToken)
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
                var rawResponse = await _service.ListPathRawAsync(
                    workspaceIdentifier,
                    itemIdentifier,
                    options.Path,
                    options.Recursive,
                    cancellationToken: cancellationToken);

                context.Response.Results = ResponseResult.Create(
                    new() { RawResponse = rawResponse },
                    MinimalJsonContext.Default.PathListResult);
                return context.Response;
            }

            List<FileSystemItem> fileSystemItems;

            // Use intelligent discovery if no path is specified
            if (string.IsNullOrWhiteSpace(options.Path))
            {
                fileSystemItems = await _service.ListPathIntelligentAsync(
                    workspaceIdentifier,
                    itemIdentifier,
                    options.Recursive,
                    cancellationToken: cancellationToken);
            }
            else
            {
                fileSystemItems = await _service.ListPathAsync(
                    workspaceIdentifier,
                    itemIdentifier,
                    options.Path,
                    options.Recursive,
                    cancellationToken: cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(
                new(fileSystemItems),
                MinimalJsonContext.Default.PathListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing path structure. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}, Path: {Path}",
                options.WorkspaceId, options.ItemId, options.Path);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record PathListResult
    {
        public List<FileSystemItem>? Items { get; init; }
        public string? RawResponse { get; init; }

        public PathListResult(List<FileSystemItem> items)
        {
            Items = items;
        }

        public PathListResult()
        {
        }
    }
}
