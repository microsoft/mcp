// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public sealed record VolumeReplicationStatus(
    [property: JsonPropertyName("errorMessage")] string? ErrorMessage,
    [property: JsonPropertyName("healthy")] bool? Healthy,
    [property: JsonPropertyName("mirrorState")] string? MirrorState,
    [property: JsonPropertyName("relationshipStatus")] string? RelationshipStatus,
    [property: JsonPropertyName("totalProgress")] string? TotalProgress);