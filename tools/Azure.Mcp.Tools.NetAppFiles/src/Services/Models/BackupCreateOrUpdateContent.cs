// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp backup via ARM generic resource API.
/// </summary>
internal sealed class BackupCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public BackupCreateProperties? Properties { get; set; }
}

internal sealed class BackupCreateProperties
{
    [JsonPropertyName("volumeResourceId")]
    public string? VolumeResourceId { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }
}
