// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Authorization.Commands;
using Azure.Mcp.Tools.Authorization.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;

namespace Azure.Mcp.Tools.Authorization;

public sealed class AuthorizationSetup : IAreaSetup
{
    public string Name => "role";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
    }

    public void RegisterCommands(CommandGroup rootGroup, ILoggerFactory loggerFactory)
    {
        // Create Authorization RBAC role command group
        var authorization = new CommandGroup(Name,
            "Authorization operations - Commands for managing Azure Role-Based Access Control (RBAC) resources. Includes operations for listing role assignments, managing permissions, and working with Azure security and access management at various scopes.");
        rootGroup.AddSubGroup(authorization);

        // Create Role Assignment subgroup
        var roleAssignment = new CommandGroup("assignment",
            "Role assignment operations - Commands for listing and managing Azure RBAC role assignments for a given scope.");
        authorization.AddSubGroup(roleAssignment);

        // Register role assignment commands
        roleAssignment.AddCommand("list", new RoleAssignmentListCommand(loggerFactory.CreateLogger<RoleAssignmentListCommand>()));
    }
}
