// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public sealed record VolumeReplicationInfo(
    [property: JsonPropertyName("endpointType")] string? EndpointType,
    [property: JsonPropertyName("mirrorState")] string? MirrorState,
    [property: JsonPropertyName("remoteVolumeRegion")] string? RemoteVolumeRegion,
    [property: JsonPropertyName("remoteVolumeResourceId")] string? RemoteVolumeResourceId,
    [property: JsonPropertyName("replicationCreationTime")] string? ReplicationCreationTime,
    [property: JsonPropertyName("replicationDeletionTime")] string? ReplicationDeletionTime,
    [property: JsonPropertyName("replicationId")] string? ReplicationId,
    [property: JsonPropertyName("replicationSchedule")] string? ReplicationSchedule);