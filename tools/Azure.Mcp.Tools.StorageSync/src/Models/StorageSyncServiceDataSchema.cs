// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.StorageSync;

namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Data transfer object for Storage Sync service information.
/// </summary>
public sealed record StorageSyncServiceDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("location")] string? Location = null,
    [property: JsonPropertyName("tags")] Dictionary<string, string>? Tags = null,
    [property: JsonPropertyName("identity")] StorageSyncServiceIdentitySchema? Identity = null,
    [property: JsonPropertyName("properties")] StorageSyncServicePropertiesSchema? Properties = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public StorageSyncServiceDataSchema() : this(null, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a StorageSyncServiceDataSchema from a StorageSyncServiceResource.
    /// </summary>
    public static StorageSyncServiceDataSchema FromResource(StorageSyncServiceResource resource)
    {
        var data = resource.Data;
        return new StorageSyncServiceDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            data.Location.ToString(),
            new Dictionary<string, string>(data.Tags ?? new Dictionary<string, string>()),
            data.Identity != null ? StorageSyncServiceIdentitySchema.FromManagedServiceIdentity(data.Identity) : null,
            new StorageSyncServicePropertiesSchema(
                data.IncomingTrafficPolicy?.ToString(),
                data.StorageSyncServiceStatus,
                data.StorageSyncServiceUid?.ToString(),
                data.ProvisioningState,
                data.UseIdentity,
                data.LastWorkflowId,
                data.LastOperationName
            )
        );
    }
}

/// <summary>
/// Storage Sync service identity.
/// </summary>
public sealed record StorageSyncServiceIdentitySchema(
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("principalId")] string? PrincipalId = null,
    [property: JsonPropertyName("tenantId")] string? TenantId = null)
{
    /// <summary>
    /// Creates a StorageSyncServiceIdentitySchema from a ManagedServiceIdentity.
    /// </summary>
    public static StorageSyncServiceIdentitySchema FromManagedServiceIdentity(Azure.ResourceManager.Models.ManagedServiceIdentity identity)
    {
        return new StorageSyncServiceIdentitySchema(
            identity.ManagedServiceIdentityType.ToString(),
            identity.PrincipalId?.ToString(),
            identity.TenantId?.ToString()
        );
    }
}

/// <summary>
/// Storage Sync service properties.
/// </summary>
public sealed record StorageSyncServicePropertiesSchema(
    [property: JsonPropertyName("incomingTrafficPolicy")] string? IncomingTrafficPolicy = null,
    [property: JsonPropertyName("storageSyncServiceStatus")] int? StorageSyncServiceStatus = null,
    [property: JsonPropertyName("storageSyncServiceUid")] string? StorageSyncServiceUid = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("useIdentity")] bool? UseIdentity = null,
    [property: JsonPropertyName("lastWorkflowId")] string? LastWorkflowId = null,
    [property: JsonPropertyName("lastOperationName")] string? LastOperationName = null)
{
}
