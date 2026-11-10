// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A receipt for deletion of the requested assignment, not a principal or effective-access snapshot.</summary>
public sealed record WorkspaceRoleAssignmentDeleteCommandResult(Guid WorkspaceId, Guid WorkspaceRoleAssignmentId)
{
    public string DeletionStatus => "Deleted";

    public string Guidance =>
        "Fabric confirmed deletion of the requested workspace role assignment (HTTP 200). " +
        "Only that assignment was removed, not the principal, group membership, workspace, or other assignments. " +
        "This does not establish that the principal has no remaining effective access. No assignment snapshot was returned. " +
        "No lookup, follow-up read, polling, or automatic retry was performed. Do not resubmit this confirmed deletion.";
}
