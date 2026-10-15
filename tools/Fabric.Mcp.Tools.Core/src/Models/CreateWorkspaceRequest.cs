// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The body of a Fabric Create Workspace request.</summary>
public sealed class CreateWorkspaceRequest()
{
    /// <summary>Gets the new workspace's display name.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Gets the optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the optional existing capacity UUID.</summary>
    public string? CapacityId { get; init; }

    /// <summary>Gets the optional existing domain UUID.</summary>
    public string? DomainId { get; init; }
}
