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
    Id = "a1b2c3d4-2001-4000-8000-000000000015",
    Name = "create_shortcut_s3_compatible",
    Title = "Create OneLake Shortcut (S3 Compatible Target)",
    Description = """
        Create a shortcut pointing to an S3-compatible storage location. Requires
        a connection ID, target URL, and bucket name. Requires OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateS3CompatibleCommand(ILogger<ShortcutCreateS3CompatibleCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ShortcutCreateS3CompatibleOptions, OneLakeShortcut>()
{
    private readonly ILogger<ShortcutCreateS3CompatibleCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShortcutCreateS3CompatibleOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var shortcut = new OneLakeShortcut
            {
                Path = options.ShortcutPath,
                Name = options.ShortcutName,
                Target = new ShortcutTarget
                {
                    S3Compatible = new S3CompatibleShortcutTarget
                    {
                        Location = options.TargetLocation,
                        Subpath = options.TargetSubpath,
                        ConnectionId = options.TargetConnectionId,
                        Bucket = options.TargetBucket
                    }
                }
            };

            var result = await _oneLakeService.CreateShortcutAsync(options.WorkspaceId, options.ItemId, shortcut, options.ShortcutConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating S3-compatible shortcut. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
