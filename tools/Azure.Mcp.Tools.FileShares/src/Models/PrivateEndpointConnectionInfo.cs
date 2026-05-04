// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.FileShares;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Lightweight projection of Private Endpoint Connection data.
/// </summary>
public sealed record PrivateEndpointConnectionInfo(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string? Type,
    [property: JsonPropertyName("privateEndpointId")] string? PrivateEndpointId,
    [property: JsonPropertyName("connectionState")] string? ConnectionState,
    [property: JsonPropertyName("connectionStateDescription")] string? ConnectionStateDescription,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState,
    [property: JsonPropertyName("groupIds")] List<string>? GroupIds)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public PrivateEndpointConnectionInfo() : this(string.Empty, string.Empty, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a PrivateEndpointConnectionInfo from a FileSharePrivateEndpointConnectionResource.
    /// </summary>
    public static PrivateEndpointConnectionInfo FromResource(FileSharePrivateEndpointConnectionResource resource)
    {
        var data = resource.Data;
        var props = data.Properties;

        return new PrivateEndpointConnectionInfo(
            Id: data.Id.ToString(),
            Name: data.Name,
            Type: data.ResourceType.ToString(),
            PrivateEndpointId: props?.PrivateEndpointId?.ToString(),
            ConnectionState: props?.PrivateLinkServiceConnectionState?.Status?.ToString(),
            ConnectionStateDescription: props?.PrivateLinkServiceConnectionState?.Description,
            ProvisioningState: props?.ProvisioningState?.ToString(),
            GroupIds: props?.GroupIds?.ToList()
        );
    }
}
