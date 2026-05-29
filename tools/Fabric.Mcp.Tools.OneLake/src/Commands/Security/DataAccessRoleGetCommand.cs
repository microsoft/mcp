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
    Id = "a1b2c3d4-1001-4000-8000-000000000002",
    Name = "get_data_access_role",
    Title = "Get OneLake Data Access Role",
    Description = """
        Get the full definition of a single data access role on a single item —
        members, permissions, decision rules. Scoped to one role on one item per
        call. Use after onelake_list_data_access_roles once you know which role
        you need on which item. Distinct from onelake_get_principal_access,
        which returns the effective (resolved) access for a given principal
        across all roles on an item. Caller must be a workspace Admin or Member
        on the item's workspace. Requires OneLake.Read.All.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class DataAccessRoleGetCommand(
    ILogger<DataAccessRoleGetCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<DataAccessRoleGetOptions>()
{
    private readonly ILogger<DataAccessRoleGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.RoleName.AsRequired());
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

    protected override DataAccessRoleGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId! : workspace ?? string.Empty;
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.RoleName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.RoleName.Name);
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


            var result = await _oneLakeService.GetDataAccessRoleAsync(options.WorkspaceId!, options.ItemId!, options.RoleName!, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DataAccessRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data access role. Workspace: {Workspace}, Item: {Item}, Role: {Role}.",
                options.WorkspaceId, options.ItemId, options.RoleName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

