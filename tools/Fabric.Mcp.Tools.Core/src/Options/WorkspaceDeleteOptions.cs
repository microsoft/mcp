// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Options for deleting one explicitly identified Fabric workspace.</summary>
public sealed class WorkspaceDeleteOptions()
{
    /// <summary>Gets or sets the workspace ID as a nonempty UUID.</summary>
    [Option(Description = "The ID of the Microsoft Fabric workspace to delete. Must be a nonempty UUID. Deleting this workspace also deletes the items under it.")]
    public required string WorkspaceId { get; set; }
}
