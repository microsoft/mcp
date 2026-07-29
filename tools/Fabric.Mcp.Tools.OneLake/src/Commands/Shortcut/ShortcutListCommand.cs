// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

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
public sealed class ShortcutListCommand(ILogger<ShortcutListCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ShortcutListOptions, ShortcutListResponse>()
{
    private readonly ILogger<ShortcutListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShortcutListOptions options, CancellationToken cancellationToken)
    {
        try
        {

            var result = await _oneLakeService.ListShortcutsAsync(options.WorkspaceId, options.ItemId, options.ParentPath, options.ContinuationToken, cancellationToken);

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

