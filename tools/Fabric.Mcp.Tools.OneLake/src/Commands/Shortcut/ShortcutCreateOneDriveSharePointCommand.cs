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
    Id = "a1b2c3d4-2001-4000-8000-000000000017",
    Name = "create_shortcut_onedrive_sharepoint",
    Title = "Create OneLake Shortcut (OneDrive/SharePoint Target)",
    Description = """
        Create a shortcut pointing to a OneDrive or SharePoint Online location.
        Requires a connection ID and target URL. Optionally updates the Fabric
        item sensitivity label from the source. Requires OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateOneDriveSharePointCommand(ILogger<ShortcutCreateOneDriveSharePointCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ShortcutCreateOneDriveSharePointOptions, OneLakeShortcut>()
{
    private readonly ILogger<ShortcutCreateOneDriveSharePointCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShortcutCreateOneDriveSharePointOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var shortcut = new OneLakeShortcut
            {
                Path = options.ShortcutPath,
                Name = options.ShortcutName,
                Target = new ShortcutTarget
                {
                    OneDriveSharePoint = new OneDriveSharePointShortcutTarget
                    {
                        Location = options.TargetLocation,
                        Subpath = options.TargetSubpath,
                        ConnectionId = options.TargetConnectionId,
                        UpdateFabricItemSensitivity = options.TargetUpdateFabricItemSensitivity ? true : null
                    }
                }
            };

            var result = await _oneLakeService.CreateShortcutAsync(options.WorkspaceId, options.ItemId, shortcut, options.ShortcutConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OneDrive/SharePoint shortcut. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
