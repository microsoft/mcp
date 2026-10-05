// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A tag applied to a Fabric workspace.</summary>
public sealed class FabricWorkspaceAppliedTag()
{
    /// <summary>Gets or sets the tag ID.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the tag display name.</summary>
    public required string DisplayName { get; set; }
}
