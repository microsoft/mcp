// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.FileShares;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Lightweight projection of FileShare data with commonly useful metadata.
/// </summary>
public sealed record FileShareInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("resourceGroup")] string? ResourceGroup,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("mountName")] string? MountName,
    [property: JsonPropertyName("hostName")] string? HostName,
    [property: JsonPropertyName("mediaTier")] string? MediaTier,
    [property: JsonPropertyName("redundancy")] string? Redundancy,
    [property: JsonPropertyName("protocol")] string? Protocol,
    [property: JsonPropertyName("provisionedStorageInGiB")] int? ProvisionedStorageInGiB,
    [property: JsonPropertyName("provisionedIOPerSec")] int? ProvisionedIOPerSec,
    [property: JsonPropertyName("provisionedThroughputMiBPerSec")] int? ProvisionedThroughputMiBPerSec,
    [property: JsonPropertyName("publicNetworkAccess")] string? PublicNetworkAccess)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public FileShareInfo() : this(string.Empty, string.Empty, null, null, null, null, null, null, null, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a FileShareInfo from a FileShareResource.
    /// </summary>
    public static FileShareInfo FromResource(FileShareResource resource)
    {
        var data = resource.Data;
        var resourceGroup = Azure.Core.ResourceIdentifier.Parse(data.Id.ToString()).ResourceGroupName;
        var props = data.Properties;

        return new FileShareInfo(
            Id: data.Id.ToString(),
            Name: data.Name,
            Location: data.Location.ToString(),
            ResourceGroup: resourceGroup,
            Type: data.ResourceType.ToString(),
            ProvisioningState: props?.ProvisioningState?.ToString(),
            MountName: props?.MountName,
            HostName: props?.HostName,
            MediaTier: props?.MediaTier?.ToString(),
            Redundancy: props?.Redundancy?.ToString(),
            Protocol: props?.Protocol?.ToString(),
            ProvisionedStorageInGiB: props?.ProvisionedStorageInGiB,
            ProvisionedIOPerSec: props?.ProvisionedIOPerSec,
            ProvisionedThroughputMiBPerSec: props?.ProvisionedThroughputMiBPerSec,
            PublicNetworkAccess: props?.PublicNetworkAccess?.ToString()
        );
    }
}
