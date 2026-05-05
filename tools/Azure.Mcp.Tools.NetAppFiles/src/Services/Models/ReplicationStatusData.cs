// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp volume with replication data from Resource Graph.
/// </summary>
internal sealed class ReplicationStatusData
{
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }

    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName("properties")]
    public ReplicationStatusProperties? Properties { get; set; }

    public static ReplicationStatusData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.ReplicationStatusData);
    }
}

internal sealed class ReplicationStatusProperties
{
    [JsonPropertyName("dataProtection")]
    public ReplicationDataProtection? DataProtection { get; set; }
}

internal sealed class ReplicationDataProtection
{
    [JsonPropertyName("replication")]
    public ReplicationInfo? Replication { get; set; }
}

internal sealed class ReplicationInfo
{
    [JsonPropertyName("endpointType")]
    public string? EndpointType { get; set; }

    [JsonPropertyName("replicationSchedule")]
    public string? ReplicationSchedule { get; set; }

    [JsonPropertyName("remoteVolumeResourceId")]
    public string? RemoteVolumeResourceId { get; set; }

    [JsonPropertyName("remoteVolumeRegion")]
    public string? RemoteVolumeRegion { get; set; }

    [JsonPropertyName("replicationId")]
    public string? ReplicationId { get; set; }
}
