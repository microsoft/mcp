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
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);

            if (string.IsNullOrWhiteSpace(workspaceId) && string.IsNullOrWhiteSpace(workspace))
            {
                result.AddError("Workspace identifier is required. Provide --workspace or --workspace-id.");
            }
        });
    }

    protected override ShortcutResetCacheOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
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

            await _oneLakeService.ResetShortcutCacheAsync(workspaceIdentifier!, cancellationToken);
            var result = new ShortcutResetCacheCommandResult("Shortcut cache reset successfully.");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ShortcutResetCacheCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting shortcut cache. Workspace: {Workspace}.",
                options.WorkspaceId ?? options.Workspace);
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
