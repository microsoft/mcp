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
    Id = "a1b2c3d4-2001-4000-8000-000000000002",
    Name = "get_shortcut",
    Title = "Get OneLake Shortcut",
    Description = """
        Get the properties of a single shortcut (name, path, target,
        configuration). Requires OneLake.Read.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class ShortcutGetCommand(ILogger<ShortcutGetCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<ShortcutGetOptions, OneLakeShortcut>()
{
    private readonly ILogger<ShortcutGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShortcutGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _oneLakeService.GetShortcutAsync(options.WorkspaceId, options.ItemId, options.ShortcutPath, options.ShortcutName, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shortcut. Workspace: {Workspace}, Item: {Item}, Path: {Path}, Name: {Name}.",
                options.WorkspaceId, options.ItemId, options.ShortcutPath, options.ShortcutName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
