// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

public sealed class BlobListCommand(
    ILogger<BlobListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<BlobListOptions>()
{
    private readonly ILogger<BlobListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "list-blobs";
    public override string Title => "List OneLake Blobs";
    public override string Description => "List files and directories in OneLake storage as blobs. Browse the contents of a lakehouse or specific directory path with optional recursive listing in blob format. If no path is specified, intelligently discovers content by searching both Files and Tables folders automatically, providing comprehensive visibility across all top-level OneLake folders. Use --format=raw to get the unprocessed OneLake API response for debugging.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Path);
        command.Options.Add(FabricOptionDefinitions.Recursive);
        command.Options.Add(OneLakeOptionDefinitions.Format);
    }

    protected override BlobListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspaceName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;

        var itemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        var itemName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);
        options.ItemId = !string.IsNullOrWhiteSpace(itemId)
            ? itemId!
            : itemName ?? string.Empty;

        options.Path = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Path.Name);
        options.Recursive = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Recursive.Name);
        options.Format = parseResult.GetValueOrDefault<string>(OneLakeOptionDefinitions.Format.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            if (string.IsNullOrWhiteSpace(options.WorkspaceId))
            {
                throw new ArgumentException("Workspace identifier is required. Provide --workspace or --workspace-id.", nameof(options.WorkspaceId));
            }

            if (string.IsNullOrWhiteSpace(options.ItemId))
            {
                throw new ArgumentException("Item identifier is required. Provide --item or --item-id.", nameof(options.ItemId));
            }

            // Check if raw format is requested
            if (options.Format?.ToLowerInvariant() == "raw")
            {
                var rawResponse = await _oneLakeService.ListBlobsRawAsync(
                    options.WorkspaceId,
                    options.ItemId,
                    options.Path,
                    options.Recursive,
                    CancellationToken.None);

                var rawResult = new BlobListCommandResult { RawResponse = rawResponse };
                context.Response.Results = ResponseResult.Create(rawResult, MinimalJsonContext.Default.BlobListCommandResult);
                return context.Response;
            }

            List<OneLakeFileInfo> files;
            
            // Use intelligent discovery if no path is specified
            if (string.IsNullOrWhiteSpace(options.Path))
            {
                files = (await _oneLakeService.ListBlobsIntelligentAsync(
                    options.WorkspaceId,
                    options.ItemId,
                    options.Recursive,
                    CancellationToken.None)).ToList();
            }
            else
            {
                files = (await _oneLakeService.ListBlobsAsync(
                    options.WorkspaceId,
                    options.ItemId,
                    options.Path,
                    options.Recursive,
                    CancellationToken.None)).ToList();
            }

            var result = new BlobListCommandResult(files, options.Path ?? "");
            context.Response.Results = ResponseResult.Create(result, MinimalJsonContext.Default.BlobListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing blobs in workspace {WorkspaceId}, item {ItemId}, path {Path}. Options: {@Options}", 
                options.WorkspaceId, options.ItemId, options.Path, options);
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