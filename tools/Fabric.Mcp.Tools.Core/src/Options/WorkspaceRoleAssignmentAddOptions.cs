// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class WorkspaceRoleAssignmentAddOptions()
{
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID; names are not resolved.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "Explicit principal JSON object with a nonempty id UUID and type: User, Group, ServicePrincipal, ServicePrincipalProfile, or EntireTenant. Supply the principal ID for that type: an Entra object ID for a user, group or service principal, or the profile ID for a service principal profile; not an application/client ID, email, name, or assignment ID. EntireTenant explicitly targets all tenant users and still requires its principal UUID. Optional writable groupDetails.groupType and servicePrincipalProfileDetails.parentPrincipal are supported for their respective types. Read-only displayName, userDetails and servicePrincipalDetails are rejected. Extensible type strings are forwarded unchanged; no identity is inferred or looked up.")]
    public required string Principal { get; set; }

    [Option(Description = "Explicit workspace role: Admin, Member, Contributor, or Viewer. No default, trimming, upgrade, or fallback. Extensible API role strings are sent unchanged. Members can grant Member or lower; granting Admin requires an Admin caller. Fabric enforces authorization.")]
    public required string Role { get; set; }
}
