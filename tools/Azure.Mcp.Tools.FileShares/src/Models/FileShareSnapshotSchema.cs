// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents a File Share Snapshot schema.
/// </summary>
public class FileShareSnapshotSchema
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource name.
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
    /// Gets or sets the file share snapshot properties.
    /// </summary>
    public FileShareSnapshotPropertiesSchema? Properties { get; set; }
}

/// <summary>
/// Represents File Share Snapshot properties schema.
/// </summary>
public class FileShareSnapshotPropertiesSchema
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
    /// Gets or sets the metadata key-value pairs.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}
