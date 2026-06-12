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
    Id = "a1b2c3d4-2001-4000-8000-000000000003",
    Name = "create_or_update_shortcuts",
    Title = "Create or Update OneLake Shortcuts",
    Description = """
        Create one or more shortcuts in a single call using the bulk create API
        (POST /shortcuts/bulkCreate). Pass a JSON object with a
        "createShortcutRequests" array — one entry for a single shortcut, many
        entries for bulk. Use shortcut conflict policy to control behaviour
        when a shortcut with the same name and path already exists: Abort
        (default), CreateOrOverwrite, OverwriteOnly, or GenerateUniqueName.
        Requires OneLake.ReadWrite.All.
        """,
    Destructive = false,
    Idempotent = false,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class ShortcutCreateOrUpdateCommand(
    ILogger<ShortcutCreateOrUpdateCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ShortcutCreateOrUpdateOptions>()
{
    private readonly ILogger<ShortcutCreateOrUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutsDefinition.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutConflictPolicy.AsOptional());
    }

    protected override ShortcutCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.ShortcutsDefinition = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutsDefinition.Name);
        options.ShortcutConflictPolicy = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ShortcutConflictPolicy.Name);
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


            var result = await _oneLakeService.CreateOrUpdateShortcutsAsync(options.WorkspaceId!, options.ItemId!, options.ShortcutsDefinition!, options.ShortcutConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.BulkCreateShortcutResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating shortcuts. Workspace: {Workspace}, Item: {Item}.",
                options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

