// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>Workspace management metadata returned by the Create Workspace API.</summary>
public sealed class CreatedWorkspaceMetadata()
{
    /// <summary>Gets the created workspace's ID.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the workspace's display name.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Gets the workspace type, including future service-defined values.</summary>
    public required string Type { get; init; }

    /// <summary>Gets the description when returned by Fabric.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the assigned capacity ID when returned by Fabric.</summary>
    public Guid? CapacityId { get; init; }

    /// <summary>Gets the capacity region, including future service-defined values.</summary>
    public string? CapacityRegion { get; init; }

    /// <summary>Gets the assigned domain ID when returned by Fabric.</summary>
    public Guid? DomainId { get; init; }

    /// <summary>Gets the returned API endpoint as metadata only; this tool does not contact it.</summary>
    public string? ApiEndpoint { get; init; }

    /// <summary>Gets the applied tags when returned by Fabric.</summary>
    public List<CreatedWorkspaceTag>? Tags { get; init; }
}
