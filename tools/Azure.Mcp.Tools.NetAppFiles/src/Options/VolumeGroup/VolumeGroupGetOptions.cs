// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.VolumeGroup;

public class VolumeGroupGetOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeGroupName)]
    public string? VolumeGroup { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }
}
