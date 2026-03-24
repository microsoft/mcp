// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp volume from Resource Graph.
/// </summary>
internal sealed class NetAppVolumeData
{
    [JsonPropertyName("id")]
    public string? ResourceId { get; set; }

    [JsonPropertyName("type")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("name")]
    public string? ResourceName { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("resourceGroup")]
    public string? ResourceGroup { get; set; }

    [JsonPropertyName("properties")]
    public NetAppVolumeProperties? Properties { get; set; }

    public static NetAppVolumeData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.NetAppVolumeData);
    }
}

internal sealed class NetAppVolumeProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("serviceLevel")]
    public string? ServiceLevel { get; set; }

    [JsonPropertyName("usageThreshold")]
    public long? UsageThreshold { get; set; }

    [JsonPropertyName("creationToken")]
    public string? CreationToken { get; set; }

    [JsonPropertyName("subnetId")]
    public string? SubnetId { get; set; }

    [JsonPropertyName("protocolTypes")]
    public List<string>? ProtocolTypes { get; set; }

    [JsonPropertyName("networkFeatures")]
    public string? NetworkFeatures { get; set; }
}
