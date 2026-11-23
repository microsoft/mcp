// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FabricWorkspaceRoleAssignment(Guid Id, FabricPrincipal Principal, string Role);
