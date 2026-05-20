// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Authorization.Commands;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Authorization;

public sealed class AuthorizationSetup : IAreaSetup
{
    public string Name => "role";

    public string Title => "Azure Role-Based Access Control";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationService, AuthorizationService>();

        services.AddSingleton<RoleAssignmentApprovalApproveCommand>();
        services.AddSingleton<RoleAssignmentApprovalListCommand>();
        services.AddSingleton<RoleAssignmentListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Authorization RBAC role command group
        var authorization = new CommandGroup(Name,
            "Authorization operations - Commands for managing Azure Role-Based Access Control (RBAC) resources. Includes operations for listing role assignments, managing permissions, and working with Azure security and access management at various scopes.", Title);

        // Create Role Assignment subgroup
        var roleAssignment = new CommandGroup("assignment",
            "Role assignment operations - Commands for listing and managing Azure RBAC role assignments for a given scope.");
        authorization.AddSubGroup(roleAssignment);

        var roleAssignmentApproval = new CommandGroup("approval",
            "Role assignment approval operations - Commands for discovering and approving Azure RBAC Privileged Identity Management (PIM) role assignment requests.");
        authorization.AddSubGroup(roleAssignmentApproval);

        // Register role assignment commands
        roleAssignment.AddCommand<RoleAssignmentListCommand>(serviceProvider);
        roleAssignmentApproval.AddCommand<RoleAssignmentApprovalApproveCommand>(serviceProvider);
        roleAssignmentApproval.AddCommand<RoleAssignmentApprovalListCommand>(serviceProvider);

        return authorization;
    }
}
