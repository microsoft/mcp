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
        Upsert a single data access role on a single item. Use flat options (--name,
        --entra-members, --permitted-paths, --permitted-actions) for the common case
        of granting Read access. For advanced scenarios (multiple decision rules,
        column/row constraints), pass the full JSON via --role-definition instead.
        When flat options are provided, --role-definition is ignored.
        Members can be specified by Entra object ID (GUID), email address, or UPN —
        non-GUID values are automatically resolved via Microsoft Graph.
        Caller must be a workspace Admin or Member. Requires OneLake.ReadWrite.All and
        User.Read.All + GroupMember.Read.All for principal resolution.
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
        command.Options.Add(FabricOptionDefinitions.RoleName.AsOptional());
        command.Options.Add(FabricOptionDefinitions.EntraMembers.AsOptional());
        command.Options.Add(FabricOptionDefinitions.FabricItemMembers.AsOptional());
        command.Options.Add(FabricOptionDefinitions.PermittedPaths.AsOptional());
        command.Options.Add(FabricOptionDefinitions.PermittedActions.AsOptional());
        command.Options.Add(FabricOptionDefinitions.RoleDefinition.AsOptional());
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

            var roleName = result.GetValueOrDefault<string>(FabricOptionDefinitions.RoleName.Name);
            var entraMembers = result.GetValueOrDefault<string>(FabricOptionDefinitions.EntraMembers.Name);
            var fabricItemMembers = result.GetValueOrDefault<string>(FabricOptionDefinitions.FabricItemMembers.Name);
            var roleDefinition = result.GetValueOrDefault<string>(FabricOptionDefinitions.RoleDefinition.Name);

            var hasFlat = !string.IsNullOrWhiteSpace(roleName) ||
                          !string.IsNullOrWhiteSpace(entraMembers) ||
                          !string.IsNullOrWhiteSpace(fabricItemMembers);

            if (!hasFlat && string.IsNullOrWhiteSpace(roleDefinition))
            {
                result.AddError("Provide either flat options (--role-name + --entra-members/--fabric-item-members) or --role-definition.");
            }

            if (hasFlat)
            {
                if (string.IsNullOrWhiteSpace(roleName))
                    result.AddError("--role-name is required when using flat options.");

                if (string.IsNullOrWhiteSpace(entraMembers) && string.IsNullOrWhiteSpace(fabricItemMembers))
                    result.AddError("At least one of --entra-members or --fabric-item-members is required.");

                if (!string.IsNullOrWhiteSpace(entraMembers))
                {
                    foreach (var member in entraMembers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        if (!Guid.TryParse(member, out _) && !member.Contains('@'))
                            result.AddError($"Invalid --entra-members value '{member}'. Must be a GUID, email, or UPN.");
                    }
                }

                var permittedActions = result.GetValueOrDefault<string>(FabricOptionDefinitions.PermittedActions.Name);
                if (!string.IsNullOrWhiteSpace(permittedActions))
                {
                    foreach (var action in permittedActions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        if (!string.Equals(action, "Read", StringComparison.OrdinalIgnoreCase))
                            result.AddError($"Unsupported --permitted-actions value '{action}'. Only 'Read' is currently supported.");
                    }
                }
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
        options.Name = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.RoleName.Name);
        options.EntraMembers = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.EntraMembers.Name);
        options.FabricItemMembers = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.FabricItemMembers.Name);
        options.PermittedPaths = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.PermittedPaths.Name);
        options.PermittedActions = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.PermittedActions.Name);
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
            DataAccessRole result;
            if (!string.IsNullOrWhiteSpace(options.Name))
            {
                // Build role from flat options
                var roleDefinitionJson = BuildRoleDefinitionJson(options);
                result = await _oneLakeService.CreateOrUpdateDataAccessRoleAsync(options.WorkspaceId!, options.ItemId!, roleDefinitionJson, cancellationToken);
            }
            else
            {
                // Use raw JSON escape hatch
                result = await _oneLakeService.CreateOrUpdateDataAccessRoleAsync(options.WorkspaceId!, options.ItemId!, options.RoleDefinition!, cancellationToken);
            }

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

    private static string BuildRoleDefinitionJson(DataAccessRoleCreateOrUpdateOptions options)
    {
        var members = new DataAccessRoleMembers();

        if (!string.IsNullOrWhiteSpace(options.EntraMembers))
        {
            members.MicrosoftEntraMembers = options.EntraMembers
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(m => new MicrosoftEntraMember { ObjectId = m })
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(options.FabricItemMembers))
        {
            members.FabricItemMembers = options.FabricItemMembers
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(m =>
                {
                    var parts = m.Split(':', 2);
                    return new FabricItemMember
                    {
                        SourcePath = parts[0],
                        ItemAccess = parts.Length > 1 ? [parts[1]] : ["Read"]
                    };
                })
                .ToList();
        }

        var actions = "Read";
        if (!string.IsNullOrWhiteSpace(options.PermittedActions))
        {
            actions = options.PermittedActions;
        }

        var permissions = new List<DecisionRuleScope>
        {
            new() { AttributeName = "Action", AttributeValueIncludedIn = actions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() }
        };

        if (!string.IsNullOrWhiteSpace(options.PermittedPaths))
        {
            permissions.Add(new DecisionRuleScope
            {
                AttributeName = "Path",
                AttributeValueIncludedIn = options.PermittedPaths.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
            });
        }

        var role = new DataAccessRole
        {
            Name = options.Name!,
            Members = members,
            DecisionRules =
            [
                new DecisionRule
                {
                    Effect = "Permit",
                    Permission = permissions
                }
            ]
        };

        return System.Text.Json.JsonSerializer.Serialize(role, OneLakeJsonContext.Default.DataAccessRole);
    }
}

