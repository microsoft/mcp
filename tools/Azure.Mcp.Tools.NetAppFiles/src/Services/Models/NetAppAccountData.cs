// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.NetAppFiles.Commands;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Internal data model representing a NetApp account from Resource Graph.
/// </summary>
internal sealed class NetAppAccountData
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
    public NetAppAccountProperties? Properties { get; set; }

    public static NetAppAccountData? FromJson(JsonElement source)
    {
        return JsonSerializer.Deserialize(source, NetAppFilesJsonContext.Default.NetAppAccountData);
    }
}

internal sealed class NetAppAccountProperties
{
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    [JsonPropertyName("activeDirectories")]
    public List<NetAppActiveDirectory>? ActiveDirectories { get; set; }

    [JsonPropertyName("encryption")]
    public NetAppEncryption? Encryption { get; set; }

    [JsonPropertyName("disableShowmount")]
    public bool? DisableShowmount { get; set; }
}

internal sealed class NetAppActiveDirectory
{
    [JsonPropertyName("activeDirectoryId")]
    public string? ActiveDirectoryId { get; set; }
}

internal sealed class NetAppEncryption
{
    [JsonPropertyName("keySource")]
    public string? KeySource { get; set; }
}
