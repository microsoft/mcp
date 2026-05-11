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
        entries for bulk. Use --shortcut-conflict-policy to control behaviour
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
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ShortcutsDefinition.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ShortcutConflictPolicy.AsOptional());
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
            var itemId = result.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
            var item = result.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);

            if (string.IsNullOrWhiteSpace(workspaceId) && string.IsNullOrWhiteSpace(workspace))
            {
                result.AddError("Workspace identifier is required. Provide --workspace or --workspace-id.");
            }

            if (string.IsNullOrWhiteSpace(item) && string.IsNullOrWhiteSpace(itemId))
            {
                result.AddError("Item identifier is required. Provide --item or --item-id.");
            }
        });
    }

    protected override ShortcutCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Item = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);
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
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            var result = await _oneLakeService.CreateOrUpdateShortcutsAsync(workspaceIdentifier!, itemIdentifier!, options.ShortcutsDefinition!, options.ShortcutConflictPolicy, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.BulkCreateShortcutResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating shortcuts. Workspace: {Workspace}, Item: {Item}.",
                options.WorkspaceId ?? options.Workspace, options.ItemId ?? options.Item);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
