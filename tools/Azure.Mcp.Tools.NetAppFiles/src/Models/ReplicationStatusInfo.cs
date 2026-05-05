// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

/// <summary>
/// Lightweight projection of a NetApp volume replication status with commonly useful metadata.
/// </summary>
public sealed record ReplicationStatusInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("endpointType")] string? EndpointType,
    [property: JsonPropertyName("replicationSchedule")] string? ReplicationSchedule,
    [property: JsonPropertyName("remoteVolumeResourceId")] string? RemoteVolumeResourceId,
    [property: JsonPropertyName("remoteVolumeRegion")] string? RemoteVolumeRegion,
    [property: JsonPropertyName("replicationId")] string? ReplicationId);
