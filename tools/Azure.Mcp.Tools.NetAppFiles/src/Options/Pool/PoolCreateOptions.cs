// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Pool;

public class PoolCreateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.PoolName)]
    public string? Pool { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SizeName)]
    public long? Size { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ServiceLevelName)]
    public string? ServiceLevel { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.QosTypeName)]
    public string? QosType { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.CoolAccessName)]
    public bool? CoolAccess { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.EncryptionTypeName)]
    public string? EncryptionType { get; set; }
}
