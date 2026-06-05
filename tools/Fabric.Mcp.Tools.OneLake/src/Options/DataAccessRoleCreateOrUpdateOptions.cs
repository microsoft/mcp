// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.OneLake.Options;

public sealed class DataAccessRoleCreateOrUpdateOptions : GlobalOptions
{
    public string? WorkspaceId { get; set; }
    public string? ItemId { get; set; }
    public string? RoleDefinition { get; set; }
    public string? Name { get; set; }
    public string? EntraMembers { get; set; }
    public string? FabricItemMembers { get; set; }
    public string? PermittedPaths { get; set; }
    public string? PermittedActions { get; set; }
}

