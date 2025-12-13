// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents Azure File Share Snapshot data.
/// </summary>
public class FileShareSnapshotData
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the snapshot name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the snapshot properties.
    /// </summary>
    public FileShareSnapshotProperties? Properties { get; set; }
}

/// <summary>
/// Represents File Share Snapshot properties.
/// </summary>
public class FileShareSnapshotProperties
{
    /// <summary>
    /// Gets or sets the snapshot time.
    /// </summary>
    public System.DateTime? SnapshotTime { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the snapshot.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// Gets or sets the provisioning state.
    /// </summary>
    public string? ProvisioningState { get; set; }
}
