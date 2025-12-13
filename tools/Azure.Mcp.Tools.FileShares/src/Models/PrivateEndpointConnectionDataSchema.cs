// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents an Azure private endpoint connection schema.
/// </summary>
public sealed class PrivateEndpointConnectionDataSchema
{
    /// <summary>
    /// Gets or sets the resource ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the system data.
    /// </summary>
    [JsonPropertyName("systemData")]
    public SystemDataSchema? SystemData { get; set; }

    /// <summary>
    /// Gets or sets the private endpoint connection properties.
    /// </summary>
    [JsonPropertyName("properties")]
    public PrivateEndpointConnectionPropertiesSchema? Properties { get; set; }
}

/// <summary>
/// Properties of a private endpoint connection schema.
/// </summary>
public sealed class PrivateEndpointConnectionPropertiesSchema
{
    /// <summary>
    /// Gets or sets the private endpoint resource.
    /// </summary>
    [JsonPropertyName("privateEndpoint")]
    public PrivateEndpointSchema? PrivateEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the group IDs for the private endpoint resource.
    /// </summary>
    [JsonPropertyName("groupIds")]
    public List<string>? GroupIds { get; set; }

    /// <summary>
    /// Gets or sets the private link service connection state.
    /// </summary>
    [JsonPropertyName("privateLinkServiceConnectionState")]
    public PrivateLinkServiceConnectionStateSchema? PrivateLinkServiceConnectionState { get; set; }

    /// <summary>
    /// Gets or sets the provisioning state.
    /// </summary>
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }
}

/// <summary>
/// Represents a private endpoint resource schema.
/// </summary>
public sealed class PrivateEndpointSchema
{
    /// <summary>
    /// Gets or sets the ARM identifier for the private endpoint.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

/// <summary>
/// State of the connection between service consumer and provider schema.
/// </summary>
public sealed class PrivateLinkServiceConnectionStateSchema
{
    /// <summary>
    /// Gets or sets the connection status (Pending, Approved, Rejected).
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Gets or sets the reason for approval/rejection of the connection.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a message indicating if changes require updates on the consumer.
    /// </summary>
    [JsonPropertyName("actionsRequired")]
    public string? ActionsRequired { get; set; }
}
