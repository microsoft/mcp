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
    Id = "a1b2c3d4-1001-4000-8000-000000000003",
    Name = "create_or_update_data_access_role",
    Title = "Create or Update OneLake Data Access Role",
    Description = """
        Upsert a single data access role on a single item. Scoped to one role on
        one item per call — does not affect other roles on the item or any roles
        on other items, so it is safe to call in a loop when multiple roles or
        multiple items need changing. There is no bulk variant: the underlying
        PUT-all API was intentionally not exposed because partial reads would
        silently delete roles. Members can be specified by Entra object ID (GUID),
        email address, or UPN — non-GUID values are automatically resolved via
        Microsoft Graph (tries /users then /groups by mail). Caller must be a
        workspace Admin or Member on the item's workspace. Requires
        OneLake.ReadWrite.All and User.Read.All + GroupMember.Read.All for
        principal resolution.
        """,
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false)]
public sealed class DataAccessRoleCreateOrUpdateCommand(
    ILogger<DataAccessRoleCreateOrUpdateCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<DataAccessRoleCreateOrUpdateOptions>()
{
    private readonly ILogger<DataAccessRoleCreateOrUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsRequired());
        command.Options.Add(FabricOptionDefinitions.RoleDefinition.AsRequired());
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

    protected override DataAccessRoleCreateOrUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId) ? workspaceId! : workspace ?? string.Empty;
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.RoleDefinition = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.RoleDefinition.Name);
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


            var result = await _oneLakeService.CreateOrUpdateDataAccessRoleAsync(options.WorkspaceId!, options.ItemId!, options.RoleDefinition!, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DataAccessRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating data access role. Workspace: {Workspace}, Item: {Item}.",
                options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

