// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record WorkspaceRoleAssignmentUpdateCommandResult(
    FabricWorkspaceRoleAssignment RoleAssignment,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string UpdateStatus => "Updated";

    public string Guidance =>
        "Fabric confirmed role-assignment update (HTTP 200). No lookup, follow-up read, creation, polling, or automatic retry was performed. Do not resubmit this update.";
}
