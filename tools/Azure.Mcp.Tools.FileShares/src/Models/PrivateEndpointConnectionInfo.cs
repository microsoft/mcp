// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.FileShares.Models;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Lightweight projection of PrivateEndpointConnection with commonly useful metadata.
/// </summary>
public sealed record PrivateEndpointConnectionInfo(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("privateEndpointId")] string? PrivateEndpointId = null,
    [property: JsonPropertyName("connectionState")] string? ConnectionState = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("groupIds")] string[]? GroupIds = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public PrivateEndpointConnectionInfo() : this(null, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a PrivateEndpointConnectionInfo from a FileSharePrivateEndpointConnection.
    /// </summary>
    public static PrivateEndpointConnectionInfo FromModel(FileSharePrivateEndpointConnection connection)
    {
        var props = connection.Properties;

        return new PrivateEndpointConnectionInfo(
            Id: connection.Id?.ToString(),
            Name: connection.Name,
            Type: connection.ResourceType.ToString(),
            PrivateEndpointId: null,
            ConnectionState: props?.PrivateLinkServiceConnectionState?.Status?.ToString(),
            ProvisioningState: props?.ProvisioningState?.ToString(),
            GroupIds: props?.GroupIds?.ToArray()
        );
    }
}
