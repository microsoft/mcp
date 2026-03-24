// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp volume group from Resource Graph.
/// </summary>
internal sealed class VolumeGroupData
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
    public VolumeGroupProperties? Properties { get; set; }

    public static VolumeGroupData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.VolumeGroupData);
    }
}

internal sealed class VolumeGroupProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("groupMetaData")]
    public VolumeGroupMetaData? GroupMetaData { get; set; }
}

internal sealed class VolumeGroupMetaData
{
    [JsonPropertyName("applicationType")]
    public string? ApplicationType { get; set; }

    [JsonPropertyName("applicationIdentifier")]
    public string? ApplicationIdentifier { get; set; }

    [JsonPropertyName("groupDescription")]
    public string? GroupDescription { get; set; }
}
