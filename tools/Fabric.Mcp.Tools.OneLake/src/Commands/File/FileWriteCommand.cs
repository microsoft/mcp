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

public sealed class FileWriteCommand(
    ILogger<FileWriteCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<FileWriteOptions>()
{
    private readonly ILogger<FileWriteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "file-write";
    public override string Title => "Write OneLake File";
    public override string Description => "Write content to a file in OneLake storage. Can write text content directly or upload from a local file.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.FilePath);
        command.Options.Add(FabricOptionDefinitions.Content);
        command.Options.Add(FabricOptionDefinitions.LocalFilePath);
        command.Options.Add(FabricOptionDefinitions.Overwrite);
    }

    protected override FileWriteOptions BindOptions(ParseResult parseResult)
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

        options.FilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FilePath.Name) ?? string.Empty;
        options.Content = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Content.Name);
        options.LocalFilePath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.LocalFilePath.Name);
        options.Overwrite = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Overwrite.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        
        try
        {
            Stream contentStream;
            long contentLength;

            // Determine content source
            if (!string.IsNullOrEmpty(options.LocalFilePath))
            {
                if (!System.IO.File.Exists(options.LocalFilePath))
                {
                    throw new FileNotFoundException($"Local file not found: {options.LocalFilePath}");
                }
                
                contentStream = System.IO.File.OpenRead(options.LocalFilePath);
                contentLength = new FileInfo(options.LocalFilePath).Length;
            }
            else if (!string.IsNullOrEmpty(options.Content))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(options.Content);
                contentStream = new MemoryStream(bytes);
                contentLength = bytes.Length;
            }
            else
            {
                throw new ArgumentException("Either --content or --local-file-path must be specified.");
            }

            if (string.IsNullOrWhiteSpace(options.WorkspaceId))
            {
                throw new ArgumentException("Workspace identifier is required. Provide --workspace or --workspace-id.", nameof(options.WorkspaceId));
            }

            if (string.IsNullOrWhiteSpace(options.ItemId))
            {
                throw new ArgumentException("Item identifier is required. Provide --item or --item-id.", nameof(options.ItemId));
            }

            using (contentStream)
            {
                await _oneLakeService.WriteFileAsync(
                    options.WorkspaceId,
                    options.ItemId,
                    options.FilePath,
                    contentStream,
                    options.Overwrite,
                    CancellationToken.None);
            }

            var result = new FileWriteCommandResult(
                options.FilePath, 
                contentLength, 
                options.Overwrite ? "File written successfully (overwritten)" : "File written successfully");
                
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.FileWriteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing file {FilePath} to workspace {WorkspaceId}, item {ItemId}. Options: {@Options}", 
                options.FilePath, options.WorkspaceId, options.ItemId, options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public sealed record FileWriteCommandResult(
        string FilePath,
        long ContentLength,
        string Message);
}

public sealed class FileWriteOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? LocalFilePath { get; set; }
    public bool Overwrite { get; set; }
}