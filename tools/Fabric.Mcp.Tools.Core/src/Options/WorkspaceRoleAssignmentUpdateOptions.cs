// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class WorkspaceRoleAssignmentUpdateOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The existing workspace role assignment ID. Must be a nonempty UUID, independent of the principal ID; no lookup or ID translation is performed.")]
    public required string WorkspaceRoleAssignmentId { get; set; }

    [Option(Description = "The explicit replacement workspace role: Admin, Member, Contributor, or Viewer. Future nonempty role strings are passed unchanged for Fabric to validate. No default, trimming, upgrade, or fallback is selected.")]
    public required string Role { get; set; }
}
