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
    Id = "a1b2c3d4-2001-4000-8000-000000000014",
    Name = "create_shortcut_gcs",
    Title = "Create OneLake Shortcut (Google Cloud Storage Target)",
    Description = """
        Create a shortcut pointing to a Google Cloud Storage location. Requires a
        connection ID for authentication and a target URL. Requires
        OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateGcsCommand(ILogger<ShortcutCreateGcsCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ShortcutCreateGcsOptions, OneLakeShortcut>()
{
    private readonly ILogger<ShortcutCreateGcsCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShortcutCreateGcsOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var shortcut = new OneLakeShortcut
            {
                Path = options.ShortcutPath,
                Name = options.ShortcutName,
                Target = new ShortcutTarget
                {
                    GoogleCloudStorage = new GoogleCloudStorageShortcutTarget
                    {
                        Location = options.TargetLocation,
                        Subpath = options.TargetSubpath,
                        ConnectionId = options.TargetConnectionId
                    }
                }
            };

            var result = await _oneLakeService.CreateShortcutAsync(options.WorkspaceId, options.ItemId, shortcut, options.ShortcutConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GCS shortcut. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
