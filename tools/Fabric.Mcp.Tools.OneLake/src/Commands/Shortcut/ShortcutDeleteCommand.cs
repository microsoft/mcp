// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.Shortcut;

[CommandMetadata(
    Id = "a1b2c3d4-2001-4000-8000-000000000004",
    Name = "delete_shortcut",
    Title = "Delete OneLake Shortcut",
    Description = """
        Delete a single shortcut from an item. Destructive but the destination
        data is preserved — only the shortcut reference is removed. Requires
        OneLake.ReadWrite.All.
        """,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutDeleteCommand(
    ILogger<ShortcutDeleteCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ShortcutDeleteOptions>()
{
    private readonly ILogger<ShortcutDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ShortcutPath.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutName.AsRequired());
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceName);
            var itemId = result.GetValueOrDefault<string>(FabricOptionDefinitions.ItemIdName);
            var item = result.GetValueOrDefault<string>(FabricOptionDefinitions.ItemName);

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

    protected override ShortcutDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceName);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemIdName);
        options.Item = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemName);
        options.ShortcutPath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutPathName);
        options.ShortcutName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutNameName);
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
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            await _oneLakeService.DeleteShortcutAsync(workspaceIdentifier!, itemIdentifier!, options.ShortcutPath!, options.ShortcutName!, cancellationToken);
            var result = new ShortcutDeleteCommandResult(options.ShortcutPath!, options.ShortcutName!, "Shortcut deleted successfully.");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ShortcutDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shortcut. Workspace: {Workspace}, Item: {Item}, Path: {Path}, Name: {Name}.",
                options.WorkspaceId ?? options.Workspace, options.ItemId ?? options.Item, options.ShortcutPath, options.ShortcutName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class ShortcutDeleteCommandResult
    {
        public string ShortcutPath { get; init; } = string.Empty;
        public string ShortcutName { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;

        public ShortcutDeleteCommandResult()
        {
        }

        public ShortcutDeleteCommandResult(string shortcutPath, string shortcutName, string message)
        {
            ShortcutPath = shortcutPath;
            ShortcutName = shortcutName;
            Message = message;
        }
    }
}
