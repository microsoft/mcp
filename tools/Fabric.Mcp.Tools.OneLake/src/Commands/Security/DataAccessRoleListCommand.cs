// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.Security;

[CommandMetadata(
    Id = "a1b2c3d4-1001-4000-8000-000000000001",
    Name = "list_data_access_roles",
    Title = "List OneLake Data Access Roles",
    Description = """
        List all data access roles defined on a single item (Lakehouse / Warehouse) —
        the role-based policies that gate Tables/Files access for that item. Scoped
        to one item per call; to inspect roles across multiple items, call once per
        item. For looking up a specific role by name, fetch the list and pick by
        name; there is no server-side search. Caller must be a workspace Admin
        or Member on the item's workspace. Requires OneLake.Read.All.
        Note: Built-in roles (e.g. DefaultReader) may include fabricItemMembers with
        a 'sourcePath' field formatted as '<workspaceId>/<itemId>' — this is NOT a
        OneLake file path; it identifies the workspace/item granting inherited access.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class DataAccessRoleListCommand(
    ILogger<DataAccessRoleListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<DataAccessRoleListOptions>()
{
    private readonly ILogger<DataAccessRoleListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ContinuationToken.AsOptional());
        command.Validators.Add(result =>
        {
            var workspaceId = result.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
            var workspace = result.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
            if (string.IsNullOrWhiteSpace(workspaceId) && string.IsNullOrWhiteSpace(workspace))
            {
                result.AddError("Workspace identifier is required. Provide --workspace or --workspace-id.");
            }

            var effectiveValue = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId : workspace;
            if (!string.IsNullOrWhiteSpace(effectiveValue) && !Guid.TryParse(effectiveValue, out _))
            {
                result.AddError("Workspace must be a valid GUID. Name-based resolution is not supported for this command.");
            }
        });
    }

    protected override DataAccessRoleListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId! : workspace ?? string.Empty;
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationTokenName);
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

            var result = await _oneLakeService.ListDataAccessRolesAsync(options.WorkspaceId!, options.ItemId!, options.ContinuationToken, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DataAccessRoleListResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing data access roles. Workspace: {Workspace}, Item: {Item}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

