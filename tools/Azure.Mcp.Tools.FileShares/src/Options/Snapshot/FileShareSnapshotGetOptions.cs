// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.Snapshot;

/// <summary>
/// Options for FileShareSnapshotGetCommand.
/// </summary>
public class FileShareSnapshotGetOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share.
    /// </summary>
    public string? FileShareName { get; set; }

    /// <summary>
    /// Gets or sets the snapshot name to retrieve.
    /// </summary>
    public string? SnapshotName { get; set; }
}
