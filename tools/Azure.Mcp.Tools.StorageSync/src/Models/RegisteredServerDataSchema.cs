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
    [property: JsonPropertyName("agentVersionStatus")] string? AgentVersionStatus = null,
    [property: JsonPropertyName("agentVersionExpireOn")] DateTimeOffset? AgentVersionExpireOn = null,
    [property: JsonPropertyName("serverManagementErrorCode")] int? ServerManagementErrorCode = null,
    [property: JsonPropertyName("lastHeartbeat")] string? LastHeartbeat = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("serverRole")] string? ServerRole = null,
    [property: JsonPropertyName("clusterId")] string? ClusterId = null,
    [property: JsonPropertyName("clusterName")] string? ClusterName = null,
    [property: JsonPropertyName("serverId")] string? ServerId = null,
    [property: JsonPropertyName("storageSyncServiceUid")] string? StorageSyncServiceUid = null,
    [property: JsonPropertyName("lastWorkflowId")] string? LastWorkflowId = null,
    [property: JsonPropertyName("lastOperationName")] string? LastOperationName = null,
    [property: JsonPropertyName("discoveryEndpointUri")] string? DiscoveryEndpointUri = null,
    [property: JsonPropertyName("resourceLocation")] string? ResourceLocation = null,
    [property: JsonPropertyName("serviceLocation")] string? ServiceLocation = null,
    [property: JsonPropertyName("friendlyName")] string? FriendlyName = null,
    [property: JsonPropertyName("managementEndpointUri")] string? ManagementEndpointUri = null,
    [property: JsonPropertyName("monitoringEndpointUri")] string? MonitoringEndpointUri = null,
    [property: JsonPropertyName("monitoringConfiguration")] string? MonitoringConfiguration = null,
    [property: JsonPropertyName("serverCertificate")] string? ServerCertificate = null,
    [property: JsonPropertyName("applicationId")] string? ApplicationId = null,
    [property: JsonPropertyName("useIdentity")] bool? UseIdentity = null,
    [property: JsonPropertyName("latestApplicationId")] string? LatestApplicationId = null,
    [property: JsonPropertyName("activeAuthType")] string? ActiveAuthType = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public RegisteredServerDataSchema() : this(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null) { }

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
            data.ServerName,
            data.ServerOSVersion,
            data.AgentVersion,
            data.AgentVersionStatus?.ToString(),
            data.AgentVersionExpireOn,
            data.ServerManagementErrorCode,
            data.LastHeartbeat,
            data.ProvisioningState,
            data.ServerRole,
            data.ClusterId?.ToString(),
            data.ClusterName,
            data.ServerId?.ToString(),
            data.StorageSyncServiceUid?.ToString(),
            data.LastWorkflowId,
            data.LastOperationName,
            data.DiscoveryEndpointUri?.ToString(),
            data.ResourceLocation?.ToString(),
            data.ServiceLocation?.ToString(),
            data.FriendlyName,
            data.ManagementEndpointUri?.ToString(),
            data.MonitoringEndpointUri?.ToString(),
            data.MonitoringConfiguration,
            data.ServerCertificate?.ToString(),
            data.ApplicationId?.ToString(),
            data.UseIdentity,
            data.LatestApplicationId?.ToString(),
            data.ActiveAuthType?.ToString()
        );
    }
}
