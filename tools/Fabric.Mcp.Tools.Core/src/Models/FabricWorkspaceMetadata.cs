// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Workspace metadata returned by the Fabric Get Workspace API, without item data.</summary>
public sealed class FabricWorkspaceMetadata()
{
    /// <summary>Gets or sets the workspace ID.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the workspace display name.</summary>
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the workspace type when returned, including future service values.</summary>
    public string? Type { get; set; }

    /// <summary>Gets or sets the description when available.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the assigned capacity ID when available.</summary>
    public Guid? CapacityId { get; set; }

    /// <summary>Gets or sets the capacity assignment progress, including future service values.</summary>
    public string? CapacityAssignmentProgress { get; set; }

    /// <summary>Gets or sets the capacity region when available.</summary>
    public string? CapacityRegion { get; set; }

    /// <summary>Gets or sets the assigned domain ID when available.</summary>
    public Guid? DomainId { get; set; }

    /// <summary>Gets or sets the workspace identity identifiers when available.</summary>
    public FabricWorkspaceIdentity? WorkspaceIdentity { get; set; }

    /// <summary>Gets or sets the OneLake endpoint metadata when available.</summary>
    public FabricWorkspaceOneLakeEndpoints? OneLakeEndpoints { get; set; }

    /// <summary>Gets or sets the workspace-specific API endpoint when returned by Fabric.</summary>
    public string? ApiEndpoint { get; set; }

    /// <summary>Gets or sets applied tags, or null when not returned by Fabric.</summary>
    public List<FabricWorkspaceAppliedTag>? Tags { get; set; }
}
