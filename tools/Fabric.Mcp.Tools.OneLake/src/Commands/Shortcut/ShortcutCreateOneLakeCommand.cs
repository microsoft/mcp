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
    Id = "a1b2c3d4-2001-4000-8000-000000000010",
    Name = "create_shortcut_onelake",
    Title = "Create OneLake Shortcut (OneLake Target)",
    Description = """
        Create a shortcut pointing to another OneLake location. Specify the target
        workspace, item, and optional path within the target item. Requires
        OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateOneLakeCommand(ILogger<ShortcutCreateOneLakeCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ShortcutCreateOneLakeOptions, OneLakeShortcut>()
{
    private readonly ILogger<ShortcutCreateOneLakeCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShortcutCreateOneLakeOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var shortcut = new OneLakeShortcut
            {
                Path = options.ShortcutPath,
                Name = options.ShortcutName,
                Target = new ShortcutTarget
                {
                    OneLake = new OneLakeShortcutTarget
                    {
                        WorkspaceId = options.TargetWorkspaceId,
                        ItemId = options.TargetItemId,
                        Path = options.TargetPath,
                        ConnectionId = options.TargetConnectionId
                    }
                }
            };

            var result = await _oneLakeService.CreateShortcutAsync(options.WorkspaceId, options.ItemId, shortcut, options.ShortcutConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating OneLake shortcut. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
