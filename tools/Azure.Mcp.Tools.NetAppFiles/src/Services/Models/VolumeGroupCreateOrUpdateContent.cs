// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp volume group via ARM generic resource API.
/// </summary>
internal sealed class VolumeGroupCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public VolumeGroupCreateProperties? Properties { get; set; }
}

internal sealed class VolumeGroupCreateProperties
{
    [JsonPropertyName("groupMetaData")]
    public VolumeGroupCreateMetaData? GroupMetaData { get; set; }
}

internal sealed class VolumeGroupCreateMetaData
{
    [JsonPropertyName("applicationType")]
    public string? ApplicationType { get; set; }

    [JsonPropertyName("applicationIdentifier")]
    public string? ApplicationIdentifier { get; set; }

    [JsonPropertyName("groupDescription")]
    public string? GroupDescription { get; set; }
}
