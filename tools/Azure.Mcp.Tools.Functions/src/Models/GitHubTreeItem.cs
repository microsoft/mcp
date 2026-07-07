// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Individual item in GitHub Tree API response.
/// </summary>
public sealed class GitHubTreeItem
{
    /// <summary>
    /// Gets or sets the path of the item relative to the repository root.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets or sets the file mode (e.g., "100644" for regular file).
    /// </summary>
    public string? Mode { get; set; }

    /// <summary>
    /// Gets or sets the type of item: "blob" for files, "tree" for directories.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the SHA of the item.
    /// </summary>
    public string? Sha { get; set; }

    /// <summary>
    /// Gets or sets the size of the file in bytes (only for blobs).
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the API URL for this item.
    /// </summary>
    public string? Url { get; set; }
}
