// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.OneLake.Commands;

public sealed class PathListCommand(ILogger<PathListCommand> logger)
    : GlobalCommand<PathListOptions>()
{
    private const string CommandTitle = "List OneLake Path Structure";
    private readonly ILogger<PathListCommand> _logger = logger;

    public override string Name => "path-list";

    public override string Description =>
        """
        List files and directories in OneLake storage using a filesystem-style hierarchical view, similar to Azure Data Lake Storage Gen2. 
        Shows directory structure with paths, sizes, timestamps, and metadata. Use this to explore OneLake content in a filesystem format 
        rather than flat blob listing. Supports optional path filtering and recursive directory traversal.
        
        If no path is specified, intelligently discovers content by searching both Files and Tables folders automatically,
        providing comprehensive visibility across all top-level OneLake folders.
        
        Use --format=raw to get the unprocessed OneLake DFS API response for debugging and analysis.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.Path.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Recursive.AsOptional());
        command.Options.Add(OneLakeOptionDefinitions.Format.AsOptional());
    }

    protected override PathListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Path = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Path.Name);
        options.Recursive = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Recursive.Name);
        options.Format = parseResult.GetValueOrDefault<string>(OneLakeOptionDefinitions.Format.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var oneLakeService = context.GetService<IOneLakeService>();
            
            // Check if raw format is requested
            if (options.Format?.ToLowerInvariant() == "raw")
            {
                var rawResponse = await oneLakeService.ListPathRawAsync(
                    options.WorkspaceId!,
                    options.ItemId!,
                    options.Path,
                    options.Recursive,
                    cancellationToken: default);

                context.Response.Results = ResponseResult.Create(
                    new PathListResult { RawResponse = rawResponse }, 
                    MinimalJsonContext.Default.PathListResult);
                return context.Response;
            }
            
            List<FileSystemItem> fileSystemItems;
            
            // Use intelligent discovery if no path is specified
            if (string.IsNullOrWhiteSpace(options.Path))
            {
                fileSystemItems = await oneLakeService.ListPathIntelligentAsync(
                    options.WorkspaceId!,
                    options.ItemId!,
                    options.Recursive,
                    cancellationToken: default);
            }
            else
            {
                fileSystemItems = await oneLakeService.ListPathAsync(
                    options.WorkspaceId!,
                    options.ItemId!,
                    options.Path,
                    options.Recursive,
                    cancellationToken: default);
            }

            context.Response.Results = ResponseResult.Create(
                new PathListResult(fileSystemItems), 
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

    internal record PathListResult
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