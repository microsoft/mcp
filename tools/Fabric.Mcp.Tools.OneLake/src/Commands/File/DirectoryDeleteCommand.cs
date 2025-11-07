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

public sealed class DirectoryDeleteCommand(
    ILogger<DirectoryDeleteCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<DirectoryDeleteOptions>()
{
    private readonly ILogger<DirectoryDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "directory-delete";
    public override string Title => "Delete OneLake Directory";
    public override string Description => "Delete a directory from OneLake storage. Use --recursive to delete non-empty directories.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
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
        command.Options.Add(FabricOptionDefinitions.DirectoryPath);
        command.Options.Add(FabricOptionDefinitions.Recursive);
    }

    protected override DirectoryDeleteOptions BindOptions(ParseResult parseResult)
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

        options.DirectoryPath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DirectoryPath.Name) ?? string.Empty;
        options.Recursive = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.Recursive.Name);
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

            await _oneLakeService.DeleteDirectoryAsync(
                options.WorkspaceId,
                options.ItemId,
                options.DirectoryPath,
                options.Recursive,
                CancellationToken.None);

            var message = options.Recursive 
                ? "Directory and all contents deleted successfully" 
                : "Directory deleted successfully";
            var result = new DirectoryDeleteCommandResult(options.DirectoryPath, message);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DirectoryDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory {DirectoryPath} from workspace {WorkspaceId}, item {ItemId}. Options: {@Options}", 
                options.DirectoryPath, options.WorkspaceId, options.ItemId, options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public sealed record DirectoryDeleteCommandResult(
        string DirectoryPath,
        string Message);
}

public sealed class DirectoryDeleteOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string DirectoryPath { get; set; } = string.Empty;
    public bool Recursive { get; set; } = false;
}