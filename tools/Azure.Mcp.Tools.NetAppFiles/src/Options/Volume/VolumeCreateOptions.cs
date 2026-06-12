// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Volume;

public class VolumeCreateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.VolumeName)]
    public string? Volume { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SubnetIdName)]
    public string? SubnetId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CreationTokenName)]
    public string? CreationToken { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.UsageThresholdName)]
    public long? UsageThreshold { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ServiceLevelName)]
    public string? ServiceLevel { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ProtocolTypesName)]
    public string[]? ProtocolTypes { get; set; }
}
