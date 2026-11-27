// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>One complete role-assignment page, not expanded group membership or effective permissions.</summary>
public sealed record WorkspaceRoleAssignmentListCommandResult(
    IReadOnlyList<FabricWorkspaceRoleAssignment> RoleAssignments,
    string? ContinuationToken = null,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null);
