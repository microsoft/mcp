// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    private const int MaxRetainedRoleAssignmentDepth = 61;
    private const string InvalidWorkspaceRoleAssignmentMessage = "Fabric returned invalid workspace role-assignment metadata.";

    private static FabricWorkspaceRoleAssignment ReadWorkspaceRoleAssignment(ReadOnlyMemory<byte> body)
    {
        using var document = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = 64, AllowDuplicateProperties = false });
        FabricCoreHttpHelpers.ValidateMetadataJson(document.RootElement);
        return ReadWorkspaceRoleAssignment(document.RootElement);
    }

    private static FabricWorkspaceRoleAssignment ReadWorkspaceRoleAssignment(JsonElement value)
    {
        var assignment = value.Deserialize(CoreJsonContext.Default.FabricWorkspaceRoleAssignment);
        if (assignment is null || assignment.Id == Guid.Empty || string.IsNullOrWhiteSpace(assignment.Role))
        {
            throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
        }
        ValidateRoleAssignmentPrincipal(assignment.Principal);
        return assignment;
    }

    private static void ValidateRoleAssignmentPrincipal(FabricPrincipal? principal)
    {
        if (principal is null || principal.Id == Guid.Empty || string.IsNullOrWhiteSpace(principal.Type) ||
            principal.ServicePrincipalDetails?.AadAppId == Guid.Empty ||
            (principal.GroupDetails?.GroupType is { } groupType && string.IsNullOrWhiteSpace(groupType)))
        {
            throw new JsonException(InvalidWorkspaceRoleAssignmentMessage);
        }
        if (principal.ServicePrincipalProfileDetails?.ParentPrincipal is { } parent)
        {
            ValidateRoleAssignmentPrincipal(parent);
        }
    }
}
