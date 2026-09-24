using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Identifies the existing Fabric item whose metadata should be retrieved.</summary>
public sealed class ItemGetOptions
{
    /// <summary>Gets or sets the containing workspace ID as a nonempty UUID string.</summary>
    [Option(Description = "The ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    /// <summary>Gets or sets the item ID as a nonempty UUID string.</summary>
    [Option(Description = "The ID of the Microsoft Fabric item to retrieve. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }
}
