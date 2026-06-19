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
    Id = "a1b2c3d4-2001-4000-8000-000000000001",
    Name = "list_shortcuts",
    Title = "List OneLake Shortcuts",
    Description = """
        List shortcuts defined within an item, recursing through subfolders.
        Returns each shortcut's path and target. Requires OneLake.Read.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class ShortcutListCommand(
    ILogger<ShortcutListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ShortcutListOptions>()
{
    private readonly ILogger<ShortcutListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ParentPath.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ContinuationToken.AsOptional());
        command.Options.Add(FabricOptionDefinitions.IncludeManaged.AsOptional());
    }

    protected override ShortcutListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.ParentPath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ParentPath.Name);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationTokenName);
        options.IncludeManaged = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.IncludeManagedName);
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

            var result = await _oneLakeService.ListShortcutsAsync(options.WorkspaceId!, options.ItemId!, options.ParentPath, options.ContinuationToken, cancellationToken);

            if (!options.IncludeManaged && result.Value is not null)
            {
                result.Value = result.Value
                    .Where(s => !IsManagedShortcut(s))
                    .ToList();
            }

            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ShortcutListResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing shortcuts. Workspace: {Workspace}, Item: {Item}.",
                options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// DW-managed shortcuts are created internally by Warehouse/SQL endpoints and can number
    /// in the hundreds of thousands, drowning user-visible shortcuts. They typically reside
    /// under well-known managed paths (e.g. "Tables/dbo.*" with OneLake-internal targets).
    /// </summary>
    private static bool IsManagedShortcut(OneLakeShortcut shortcut)
    {
        // Managed shortcuts are internal OneLake-to-OneLake references under DW table paths.
        // Heuristic: shortcuts whose path starts with "Tables/" and target is OneLake are DW-managed.
        if (shortcut.Path is null || shortcut.Target?.OneLake is null)
            return false;

        return shortcut.Path.StartsWith("Tables/", StringComparison.OrdinalIgnoreCase);
    }
}

