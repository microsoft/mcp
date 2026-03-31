// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.File;

public sealed class PathListCommand(ILogger<PathListCommand> logger)
    : GlobalCommand<PathListOptions>()
{
    private readonly ILogger<PathListCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Path.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Recursive.AsOptional());
        command.Options.Add(OneLakeOptionDefinitions.Format.AsOptional());
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
            var itemId = result.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
            var item = result.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);

            if (string.IsNullOrWhiteSpace(workspaceId) && string.IsNullOrWhiteSpace(workspace))
            {
                result.AddError("Workspace identifier is required. Provide --workspace or --workspace-id.");
            }

            if (string.IsNullOrWhiteSpace(item) && string.IsNullOrWhiteSpace(itemId))
            {
                result.AddError("Item identifier is required. Provide --item or --item-id.");
            }
        });
    }

    protected override PathListOptions BindOptions(ParseResult parseResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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
                fileSystemItems = await oneLakeService.ListPathIntelligentAsync(
                    options.WorkspaceId!,
                    options.ItemId!,
                    options.Recursive,
                    cancellationToken: cancellationToken);
            }
            else
            {
                fileSystemItems = await oneLakeService.ListPathAsync(
                    options.WorkspaceId!,
                    options.ItemId!,
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
