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
    Id = "a1b2c3d4-2001-4000-8000-000000000005",
    Name = "reset_shortcut_cache",
    Title = "Reset OneLake Shortcut Cache",
    Description = """
        Drop cached shortcut reads for an item, forcing the next read to
        re-resolve from the destination. Use sparingly — primarily for debugging
        stale-cache issues. Requires OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutResetCacheCommand(
    ILogger<ShortcutResetCacheCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ShortcutResetCacheOptions>()
{
    private readonly ILogger<ShortcutResetCacheCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
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

    protected override ShortcutResetCacheOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceName);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemIdName);
        options.Item = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemName);
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

            await _oneLakeService.ResetShortcutCacheAsync(workspaceIdentifier!, itemIdentifier!, cancellationToken);
            var result = new ShortcutResetCacheCommandResult("Shortcut cache reset successfully.");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ShortcutResetCacheCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting shortcut cache. Workspace: {Workspace}, Item: {Item}.",
                options.WorkspaceId ?? options.Workspace, options.ItemId ?? options.Item);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class ShortcutResetCacheCommandResult
    {
        public string Message { get; init; } = string.Empty;

        public ShortcutResetCacheCommandResult()
        {
        }

        public ShortcutResetCacheCommandResult(string message)
        {
            Message = message;
        }
    }
}
