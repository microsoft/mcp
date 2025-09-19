// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Authorization.Commands;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Authorization;

public sealed class AuthorizationSetup : IAreaSetup
{
    public string Name => "role";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationService, AuthorizationService>();

        services.AddSingleton<RoleAssignmentListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Authorization RBAC role command group
        var authorization = new CommandGroup(Name,
            "Authorization operations - Commands for managing Azure Role-Based Access Control (RBAC) resources. Includes operations for listing role assignments, managing permissions, and working with Azure security and access management at various scopes.");

        // Create Role Assignment subgroup
        var roleAssignment = new CommandGroup("assignment",
            "Role assignment operations - Commands for listing and managing Azure RBAC role assignments for a given scope.");
        authorization.AddSubGroup(roleAssignment);

        // Register role assignment commands
        var command = serviceProvider.GetRequiredService<RoleAssignmentListCommand>();
        roleAssignment.AddCommand(command.Name, command);

        return authorization;
    }
}
