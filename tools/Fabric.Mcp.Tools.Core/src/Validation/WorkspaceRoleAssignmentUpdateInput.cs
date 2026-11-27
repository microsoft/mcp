// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class WorkspaceRoleAssignmentUpdateInput
{
    internal static (Guid WorkspaceId, Guid AssignmentId, string Body) CreateRequest(
        string workspaceId, string workspaceRoleAssignmentId, string role, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new FabricItemInputException("--workspace-id must be a nonempty UUID.");
        }
        if (!Guid.TryParse(workspaceRoleAssignmentId, out var assignment) || assignment == Guid.Empty)
        {
            throw new FabricItemInputException("--workspace-role-assignment-id must be a nonempty UUID identifying the existing assignment, not its principal.");
        }
        WorkspaceRoleAssignmentInput.ValidateRole(role);
        var body = JsonSerializer.Serialize(new UpdateWorkspaceRoleAssignmentRequest(role),
            CoreJsonContext.Default.UpdateWorkspaceRoleAssignmentRequest);
        WorkspaceRoleAssignmentInput.ValidateRequestSize(body);
        cancellationToken.ThrowIfCancellationRequested();
        return (workspace, assignment, body);
    }
}
