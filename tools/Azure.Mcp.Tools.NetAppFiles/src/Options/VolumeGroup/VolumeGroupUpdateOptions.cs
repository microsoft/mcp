// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public class VolumeGroupUpdateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeGroupName)]
    public string? VolumeGroup { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.GroupDescriptionName)]
    public string? GroupDescription { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }
}
