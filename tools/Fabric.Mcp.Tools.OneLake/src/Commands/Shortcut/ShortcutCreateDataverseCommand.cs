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
    Id = "a1b2c3d4-2001-4000-8000-000000000016",
    Name = "create_shortcut_dataverse",
    Title = "Create OneLake Shortcut (Dataverse Target)",
    Description = """
        Create a shortcut pointing to a Dataverse environment. Requires the
        environment domain, connection ID, and Delta Lake folder. Requires
        OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateDataverseCommand(ILogger<ShortcutCreateDataverseCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ShortcutCreateDataverseOptions, OneLakeShortcut>()
{
    private readonly ILogger<ShortcutCreateDataverseCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShortcutCreateDataverseOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var shortcut = new OneLakeShortcut
            {
                Path = options.ShortcutPath,
                Name = options.ShortcutName,
                Target = new ShortcutTarget
                {
                    Dataverse = new DataverseShortcutTarget
                    {
                        EnvironmentDomain = options.TargetEnvironmentDomain,
                        ConnectionId = options.TargetConnectionId,
                        DeltaLakeFolder = options.TargetDeltalakeFolder
                    }
                }
            };

            var result = await _oneLakeService.CreateShortcutAsync(options.WorkspaceId, options.ItemId, shortcut, options.ShortcutConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Dataverse shortcut. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
