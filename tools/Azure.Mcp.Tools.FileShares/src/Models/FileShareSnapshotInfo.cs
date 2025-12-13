// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Lightweight projection of FileShare Snapshot with commonly useful metadata.
/// </summary>
public sealed record FileShareSnapshotInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("snapshotTime")] string? SnapshotTime,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup);
