// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

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
public sealed class DataAccessRoleCreateOrUpdateCommand(ILogger<DataAccessRoleCreateOrUpdateCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<DataAccessRoleCreateOrUpdateOptions, DataAccessRole>()
{
    private readonly ILogger<DataAccessRoleCreateOrUpdateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(DataAccessRoleCreateOrUpdateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }

        var effectiveValue = !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace;
        if (!string.IsNullOrWhiteSpace(effectiveValue) && !Guid.TryParse(effectiveValue, out _))
        {
            validationResult.Errors.Add("Workspace must be a valid GUID. Name-based resolution is not supported for this command.");
        }

        var hasFlat = !string.IsNullOrWhiteSpace(options.RoleName) ||
                      !string.IsNullOrWhiteSpace(options.EntraMembers) ||
                      !string.IsNullOrWhiteSpace(options.FabricItemMembers);

        if (!hasFlat && string.IsNullOrWhiteSpace(options.RoleDefinition))
        {
            validationResult.Errors.Add("Provide either flat options (--role-name + --entra-members/--fabric-item-members) or --role-definition.");
        }

        if (hasFlat)
        {
            if (string.IsNullOrWhiteSpace(options.RoleName))
            {
                validationResult.Errors.Add("--role-name is required when using flat options.");
            }

            if (string.IsNullOrWhiteSpace(options.EntraMembers) && string.IsNullOrWhiteSpace(options.FabricItemMembers))
            {
                validationResult.Errors.Add("At least one of --entra-members or --fabric-item-members is required.");
            }

            if (!string.IsNullOrWhiteSpace(options.EntraMembers))
            {
                foreach (var member in options.EntraMembers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!Guid.TryParse(member, out _) && !member.Contains('@'))
                    {
                        validationResult.Errors.Add($"Invalid --entra-members value '{member}'. Must be a GUID, email, or UPN.");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(options.PermittedActions))
            {
                foreach (var action in options.PermittedActions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!string.Equals(action, "Read", StringComparison.OrdinalIgnoreCase))
                    {
                        validationResult.Errors.Add($"Unsupported --permitted-actions value '{action}'. Only 'Read' is currently supported.");
                    }
                }
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DataAccessRoleCreateOrUpdateOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.Workspace : options.WorkspaceId;
        try
        {
            DataAccessRole result;
            if (!string.IsNullOrWhiteSpace(options.RoleName))
            {
                // Build role from flat options
                var roleDefinitionJson = BuildRoleDefinitionJson(options);
                result = await _oneLakeService.CreateOrUpdateDataAccessRoleAsync(workspaceId!, options.ItemId, roleDefinitionJson, cancellationToken);
            }
            else
            {
                // Use raw JSON escape hatch
                result = await _oneLakeService.CreateOrUpdateDataAccessRoleAsync(workspaceId!, options.ItemId, options.RoleDefinition!, cancellationToken);
            }

            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.DataAccessRole);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating data access role. Workspace: {Workspace}, Item: {Item}.",
                workspaceId, options.ItemId);
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
            Name = options.RoleName!,
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

        return JsonSerializer.Serialize(role, OneLakeJsonContext.Default.DataAccessRole);
    }
}
