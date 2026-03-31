// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp snapshot via ARM generic resource API.
/// </summary>
internal sealed class SnapshotCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public SnapshotCreateProperties? Properties { get; set; }
}

internal sealed class SnapshotCreateProperties
{
}
