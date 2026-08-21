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

    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AddName)]
    public string[]? Add { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SetName)]
    public string[]? Set { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoveName)]
    public string[]? Remove { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ForceStringName)]
    public bool ForceString { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.GroupMetaDataName)]
    public string? GroupMetaData { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumesName)]
    public string? Volumes { get; set; }
}
