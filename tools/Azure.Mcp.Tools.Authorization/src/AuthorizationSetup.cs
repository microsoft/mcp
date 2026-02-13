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

        services.AddSingleton<RoleAssignmentListCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        // Create Authorization RBAC role command group
        var authorization = new CommandGroup(Name,
            "Authorization operations – Commands for managing Azure RBAC resources, including role assignments, permission management, and access control at subscription, resource group, and resource scopes.", Title);

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
