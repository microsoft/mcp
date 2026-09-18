namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// Generic Fabric item metadata, excluding data, definitions, and workload-specific properties.
/// </summary>
public sealed class FabricItemMetadata
{
    /// <summary>Gets or sets the item's unique ID.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the item's display name.</summary>
    public required string DisplayName { get; set; }

    /// <summary>Gets or sets the item description, or <see langword="null"/> when unavailable.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the Fabric item type, including types introduced by future service versions.</summary>
    public required string Type { get; set; }

    /// <summary>Gets or sets the ID of the workspace containing the item.</summary>
    public required Guid WorkspaceId { get; set; }
}
