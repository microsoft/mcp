// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// GitHub Tree API response model.
/// </summary>
public sealed class GitHubTreeResponse
{
    /// <summary>
    /// Gets or sets the SHA of the tree.
    /// </summary>
    public string? Sha { get; set; }

    /// <summary>
    /// Gets or sets the URL of the tree.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Gets or sets the list of items in the tree.
    /// </summary>
    public List<GitHubTreeItem> Tree { get; set; } = [];

    /// <summary>
    /// Gets or sets whether the tree was truncated due to size.
    /// When true, the tree response is incomplete and some files may be missing.
    /// </summary>
    public bool Truncated { get; set; }
}
