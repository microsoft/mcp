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
    Id = "a1b2c3d4-2001-4000-8000-000000000018",
    Name = "create_shortcut_external_data_share",
    Title = "Create OneLake Shortcut (External Data Share Target)",
    Description = """
        Create a shortcut pointing to an external data share. Only requires a
        connection ID. Requires OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateExternalDataShareCommand(
    ILogger<ShortcutCreateExternalDataShareCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ShortcutCreateOptions>()
{
    private readonly ILogger<ShortcutCreateExternalDataShareCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutPath.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutName.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutConflictPolicy.AsOptional());
        command.Options.Add(FabricOptionDefinitions.TargetConnectionId.AsRequired());
    }

    protected override ShortcutCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Path = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutPath.Name);
        options.Name = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutName.Name);
        options.ConflictPolicy = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutConflictPolicy.Name);
        options.TargetConnectionId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.TargetConnectionId.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            return context.Response;

        var options = BindOptions(parseResult);
        try
        {
            var shortcut = new OneLakeShortcut
            {
                Path = options.Path,
                Name = options.Name,
                Target = new ShortcutTarget
                {
                    Type = "ExternalDataShare",
                    ExternalDataShare = new ExternalDataShareShortcutTarget
                    {
                        ConnectionId = options.TargetConnectionId
                    }
                }
            };

            var result = await _oneLakeService.CreateShortcutAsync(options.WorkspaceId!, options.ItemId!, shortcut, options.ConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.OneLakeShortcut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating external data share shortcut. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
