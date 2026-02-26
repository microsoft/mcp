// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.FileShares;
using Azure.ResourceManager.FileShares.Models;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Data transfer object for private endpoint connection information.
/// </summary>
public sealed record PrivateEndpointConnectionDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("systemData")] SystemDataSchema? SystemData = null,
    [property: JsonPropertyName("properties")] PrivateEndpointConnectionPropertiesSchema? Properties = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public PrivateEndpointConnectionDataSchema() : this(null, null, null, null, null) { }

    /// <summary>
    /// Creates a PrivateEndpointConnectionDataSchema from a FileSharePrivateEndpointConnectionData.
    /// </summary>
    public static PrivateEndpointConnectionDataSchema FromModel(FileSharePrivateEndpointConnectionData connection)
    {
        var props = connection.Properties;

        return new PrivateEndpointConnectionDataSchema(
            connection.Id?.ToString(),
            connection.Name,
            connection.ResourceType.ToString(),
            connection.SystemData != null ? SystemDataSchema.FromSystemData(connection.SystemData) : null,
            props != null ? new PrivateEndpointConnectionPropertiesSchema(
                null,
                props.GroupIds?.ToList(),
                props.PrivateLinkServiceConnectionState != null ? new PrivateLinkServiceConnectionStateSchema(
                    props.PrivateLinkServiceConnectionState.Status?.ToString(),
                    props.PrivateLinkServiceConnectionState.Description,
                    props.PrivateLinkServiceConnectionState.ActionsRequired
                ) : null,
                props.ProvisioningState?.ToString()
            ) : null
        );
    }
}

/// <summary>
/// Properties of a private endpoint connection schema.
/// </summary>
public sealed record PrivateEndpointConnectionPropertiesSchema(
    [property: JsonPropertyName("privateEndpoint")] PrivateEndpointSchema? PrivateEndpoint = null,
    [property: JsonPropertyName("groupIds")] List<string>? GroupIds = null,
    [property: JsonPropertyName("privateLinkServiceConnectionState")] PrivateLinkServiceConnectionStateSchema? PrivateLinkServiceConnectionState = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public PrivateEndpointConnectionPropertiesSchema() : this(null, null, null, null) { }
}

/// <summary>
/// Represents a private endpoint resource schema.
/// </summary>
public sealed record PrivateEndpointSchema(
    [property: JsonPropertyName("id")] string? Id = null);

/// <summary>
/// State of the connection between service consumer and provider schema.
/// </summary>
public sealed record PrivateLinkServiceConnectionStateSchema(
    [property: JsonPropertyName("status")] string? Status = null,
    [property: JsonPropertyName("description")] string? Description = null,
    [property: JsonPropertyName("actionsRequired")] string? ActionsRequired = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public PrivateLinkServiceConnectionStateSchema() : this(null, null, null) { }
}
