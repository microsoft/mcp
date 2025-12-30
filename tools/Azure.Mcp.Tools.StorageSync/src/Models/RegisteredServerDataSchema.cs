// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.StorageSync;

namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Data transfer object for Registered Server information.
/// </summary>
public sealed record RegisteredServerDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("serverName")] string? ServerName = null,
    [property: JsonPropertyName("serverOsVersion")] string? ServerOSVersion = null,
    [property: JsonPropertyName("agentVersion")] string? AgentVersion = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("serverRole")] string? ServerRole = null,
    [property: JsonPropertyName("clusterName")] string? ClusterName = null,
    [property: JsonPropertyName("serverCertificate")] string? ServerCertificate = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public RegisteredServerDataSchema() : this(null, null, null, null, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a RegisteredServerDataSchema from a StorageSyncRegisteredServerResource.
    /// </summary>
    public static RegisteredServerDataSchema FromResource(StorageSyncRegisteredServerResource resource)
    {
        var data = resource.Data;
        return new RegisteredServerDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            null,
            null,
            data.AgentVersion,
            null,
            null,
            null,
            data.ServerCertificate?.ToString()
        );
    }
}
