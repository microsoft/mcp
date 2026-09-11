// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp backup vault from Resource Graph.
/// </summary>
internal sealed class BackupVaultData
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
    public BackupVaultProperties? Properties { get; set; }

    public static BackupVaultData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.BackupVaultData);
    }
}

internal sealed class BackupVaultProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }
}
