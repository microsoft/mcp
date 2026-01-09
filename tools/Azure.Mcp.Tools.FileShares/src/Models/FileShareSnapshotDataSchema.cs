// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents Azure File Share Snapshot data.
/// </summary>
public class FileShareSnapshotDataSchema
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
    /// Gets or sets the system data.
    /// </summary>
    [JsonPropertyName("systemData")]
    public SystemDataSchema? SystemData { get; set; }

    /// <summary>
    /// Gets or sets the snapshot properties.
    /// </summary>
    public FileShareSnapshotDataPropertiesSchema? Properties { get; set; }
}

/// <summary>
/// Represents File Share Snapshot properties for the data model.
/// </summary>
public class FileShareSnapshotDataPropertiesSchema
{
    /// <summary>
    /// Gets or sets the snapshot time in UTC.
    /// </summary>
    [JsonPropertyName("snapshotTime")]
    public string? SnapshotTime { get; set; }

    /// <summary>
    /// Gets or sets the initiator ID (user-defined value).
    /// </summary>
    [JsonPropertyName("initiatorId")]
    public string? InitiatorId { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with the snapshot.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
