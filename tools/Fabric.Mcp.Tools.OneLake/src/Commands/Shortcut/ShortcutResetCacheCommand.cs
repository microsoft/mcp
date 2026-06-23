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
        Drop cached shortcut reads for a workspace, forcing the next read to
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
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsRequired());
    }

    protected override ShortcutResetCacheOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
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

            await _oneLakeService.ResetShortcutCacheAsync(options.WorkspaceId!, cancellationToken);
            var result = new ShortcutResetCacheCommandResult("Shortcut cache reset successfully.");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ShortcutResetCacheCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting shortcut cache. Workspace: {Workspace}.",
                options.WorkspaceId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ShortcutResetCacheCommandResult(string Message);
}

