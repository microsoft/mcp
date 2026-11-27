// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class WorkspaceRoleAssignmentGetOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The workspace role assignment ID. Must be a nonempty UUID. This identifies the assignment, not its principal; no lookup or ID translation is performed.")]
    public required string WorkspaceRoleAssignmentId { get; set; }
}
