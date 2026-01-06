// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Options.Snapshot;

/// <summary>
/// Options for FileShareSnapshotCreateCommand.
/// </summary>
public class FileShareSnapshotCreateOptions : BaseFileSharesOptions
{
    /// <summary>
    /// Gets or sets the name of the file share to create a snapshot of.
    /// </summary>
    public string? FileShareName { get; set; }

    /// <summary>
    /// Gets or sets the snapshot name.
    /// </summary>
    public string? SnapshotName { get; set; }

    /// <summary>
    /// Gets or sets the initiator ID (user-defined value).
    /// </summary>
    public string? InitiatorId { get; set; }

    /// <summary>
    /// Gets or sets the metadata (JSON format or comma-separated key=value pairs).
    /// </summary>
    public string? Metadata { get; set; }
}
