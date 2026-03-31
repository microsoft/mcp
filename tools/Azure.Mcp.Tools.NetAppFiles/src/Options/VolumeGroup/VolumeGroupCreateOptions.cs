// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public class VolumeGroupCreateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeGroupName)]
    public string? VolumeGroup { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ApplicationTypeName)]
    public string? ApplicationType { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ApplicationIdentifierName)]
    public string? ApplicationIdentifier { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.GroupDescriptionName)]
    public string? GroupDescription { get; set; }
}
