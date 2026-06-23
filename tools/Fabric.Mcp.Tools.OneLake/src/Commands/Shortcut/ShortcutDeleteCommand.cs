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
    Id = "a1b2c3d4-2001-4000-8000-000000000004",
    Name = "delete_shortcut",
    Title = "Delete OneLake Shortcut",
    Description = """
        Delete a single shortcut from an item. Destructive but the destination
        data is preserved — only the shortcut reference is removed. Requires
        OneLake.ReadWrite.All.
        """,
    Destructive = true,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutDeleteCommand(
    ILogger<ShortcutDeleteCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ShortcutDeleteOptions>()
{
    private readonly ILogger<ShortcutDeleteCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutPath.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutName.AsRequired());
    }

    protected override ShortcutDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.ShortcutPath = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutPath.Name);
        options.ShortcutName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutName.Name);
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


            await _oneLakeService.DeleteShortcutAsync(options.WorkspaceId!, options.ItemId!, options.ShortcutPath!, options.ShortcutName!, cancellationToken);
            var result = new ShortcutDeleteCommandResult(options.ShortcutPath!, options.ShortcutName!, "Shortcut deleted successfully.");
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ShortcutDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting shortcut. Workspace: {Workspace}, Item: {Item}, Path: {Path}, Name: {Name}.",
                options.WorkspaceId, options.ItemId, options.ShortcutPath, options.ShortcutName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ShortcutDeleteCommandResult(string ShortcutPath, string ShortcutName, string Message);
}

