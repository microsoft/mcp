// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.FileShares;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Data transfer object for File Share Snapshot information.
/// </summary>
public sealed record FileShareSnapshotDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("systemData")] SystemDataSchema? SystemData = null,
    [property: JsonPropertyName("properties")] FileShareSnapshotDataPropertiesSchema? Properties = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public FileShareSnapshotDataSchema() : this(null, null, null, null, null) { }

    /// <summary>
    /// Creates a FileShareSnapshotDataSchema from a FileShareSnapshotResource.
    /// </summary>
    public static FileShareSnapshotDataSchema FromResource(FileShareSnapshotResource resource)
    {
        var data = resource.Data;
        var props = data.Properties;

        return new FileShareSnapshotDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            data.SystemData != null ? SystemDataSchema.FromSystemData(data.SystemData) : null,
            props != null ? new FileShareSnapshotDataPropertiesSchema(
                props.SnapshotTime,
                props.InitiatorId,
                props.Metadata != null ? new Dictionary<string, string>(props.Metadata) : null
            ) : null
        );
    }
}

/// <summary>
/// Represents File Share Snapshot properties for the data model.
/// </summary>
public sealed record FileShareSnapshotDataPropertiesSchema(
    [property: JsonPropertyName("snapshotTime")] string? SnapshotTime = null,
    [property: JsonPropertyName("initiatorId")] string? InitiatorId = null,
    [property: JsonPropertyName("metadata")] Dictionary<string, string>? Metadata = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public FileShareSnapshotDataPropertiesSchema() : this(null, null, null) { }
}
