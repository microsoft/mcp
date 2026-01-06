// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.FileShares;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Lightweight projection of FileShare Snapshot with commonly useful metadata.
/// </summary>
public sealed record FileShareSnapshotInfo(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("snapshotTime")] string? SnapshotTime = null,
    [property: JsonPropertyName("initiatorId")] string? InitiatorId = null,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public FileShareSnapshotInfo() : this(null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a FileShareSnapshotInfo from a FileShareSnapshotResource.
    /// </summary>
    public static FileShareSnapshotInfo FromResource(FileShareSnapshotResource resource)
    {
        var data = resource.Data;
        var resourceGroup = ExtractResourceGroupFromId(data.Id.ToString());
        var props = data.Properties;

        return new FileShareSnapshotInfo(
            Id: data.Id.ToString(),
            Name: data.Name,
            Type: data.ResourceType.ToString(),
            SnapshotTime: props?.SnapshotTime,
            InitiatorId: props?.InitiatorId,
            ResourceGroup: resourceGroup
        );
    }

    /// <summary>
    /// Extracts resource group name from Azure resource ID.
    /// </summary>
    private static string? ExtractResourceGroupFromId(string resourceId)
    {
        const string pattern = "/resourcegroups/";
        var index = resourceId.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var start = index + pattern.Length;
        var end = resourceId.IndexOf('/', start);
        if (end < 0)
        {
            end = resourceId.Length;
        }

        return resourceId.Substring(start, end - start);
    }
}
