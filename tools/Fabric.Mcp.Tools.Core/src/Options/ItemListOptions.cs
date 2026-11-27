// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

/// <summary>Options for listing one page of Fabric item metadata.</summary>
public sealed class ItemListOptions
{
    /// <summary>Gets or sets the workspace UUID.</summary>
    [Option(Description = "The Microsoft Fabric workspace ID. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    /// <summary>Gets or sets the optional item-type filter.</summary>
    [Option(Description = "Filter by Fabric item type, such as Lakehouse, Notebook, or Report. Omit to include all types.")]
    public string? Type { get; set; }

    /// <summary>Gets or sets whether nested folders are included.</summary>
    [Option(Description = "Include items in nested folders. Defaults to true; false lists only direct items in the workspace root or specified root folder.")]
    public bool? Recursive { get; set; }

    /// <summary>Gets or sets the optional root folder UUID.</summary>
    [Option(Description = "The root folder ID to list within. Must be a nonempty UUID. Omit to use the workspace root.")]
    public string? RootFolderId { get; set; }

    /// <summary>Gets or sets the token for the next page.</summary>
    [Option(Description = "The continuation token returned by a previous page, copied unchanged. Keep the same workspace, type, root folder, and recursive options. Omit for the first page.")]
    public string? ContinuationToken { get; set; }
}
