namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The metadata-only result of the Fabric get-item command.</summary>
/// <param name="Item">The metadata for the requested item.</param>
public sealed record ItemGetCommandResult(FabricItemMetadata Item);
