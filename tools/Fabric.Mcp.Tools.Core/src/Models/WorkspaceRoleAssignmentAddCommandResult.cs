// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record WorkspaceRoleAssignmentAddCommandResult(
    FabricWorkspaceRoleAssignment RoleAssignment,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null)
{
    public string CreationStatus => "Created";

    public string Guidance =>
        "Fabric confirmed role-assignment creation (HTTP 201). No lookup, follow-up read, polling, automatic retry, or update was performed. Do not resubmit this grant.";
}
