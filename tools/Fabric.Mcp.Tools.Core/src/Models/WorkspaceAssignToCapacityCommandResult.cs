// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record WorkspaceAssignToCapacityCommandResult(
    Guid WorkspaceId,
    [property: Description("The requested target capacity ID, not a verified current assignment.")]
    Guid CapacityId)
{
    [Description("The service accepted the request; this does not confirm completion.")]
    public bool Accepted => true;

    [Description("The submitted assignment is pending. No completion check was performed.")]
    public string State => "Pending";
}
