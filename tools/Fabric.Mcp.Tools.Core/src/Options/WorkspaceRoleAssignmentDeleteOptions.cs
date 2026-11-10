// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class WorkspaceRoleAssignmentDeleteOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The existing workspace role assignment ID to delete. Must be a nonempty UUID, independent of the principal ID; no lookup or ID translation is performed.")]
    public required string WorkspaceRoleAssignmentId { get; set; }
}
