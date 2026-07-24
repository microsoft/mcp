// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp backup from Resource Graph.
/// </summary>
internal sealed class BackupData
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
    public BackupProperties? Properties { get; set; }

    public static BackupData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.BackupData);
    }
}

internal sealed class BackupProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("backupType")]
    public string? BackupType { get; set; }

    [JsonPropertyName("size")]
    public long? Size { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("creationDate")]
    public string? CreationDate { get; set; }
}
