// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class WorkspaceAssignToCapacityOptions
{
    [Option(Description = "The ID of the existing Microsoft Fabric workspace, as a nonempty GUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The ID of the target capacity, as a nonempty GUID.")]
    public required string CapacityId { get; set; }
}
