// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp volume via ARM generic resource API.
/// </summary>
internal sealed class NetAppVolumeCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("properties")]
    public NetAppVolumeCreateProperties? Properties { get; set; }

    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
}

internal sealed class NetAppVolumeCreateProperties
{
    [JsonPropertyName("creationToken")]
    public string? CreationToken { get; set; }

    [JsonPropertyName("usageThreshold")]
    public long? UsageThreshold { get; set; }

    [JsonPropertyName("subnetId")]
    public string? SubnetId { get; set; }

    [JsonPropertyName("serviceLevel")]
    public string? ServiceLevel { get; set; }

    [JsonPropertyName("protocolTypes")]
    public List<string>? ProtocolTypes { get; set; }
}
