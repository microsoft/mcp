// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.Snapshot;

/// <summary>
/// Options for FileShareSnapshotListCommand.
/// </summary>
public class FileShareSnapshotListOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share to list snapshots for.
    /// </summary>
    public string? FileShareName { get; set; }
}
