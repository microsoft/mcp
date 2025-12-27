// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.StorageSync;

namespace Azure.Mcp.Tools.StorageSync.Models;

/// <summary>
/// Data transfer object for Cloud Endpoint information.
/// </summary>
public sealed record CloudEndpointDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("storageAccountResourceId")] string? StorageAccountResourceId = null,
    [property: JsonPropertyName("azureFileShareName")] string? AzureFileShareName = null,
    [property: JsonPropertyName("storageAccountTenantId")] string? StorageAccountTenantId = null,
    [property: JsonPropertyName("partnershipId")] string? PartnershipId = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("lastOperationName")] string? LastOperationName = null,
    [property: JsonPropertyName("lastWorkflowId")] string? LastWorkflowId = null,
    [property: JsonPropertyName("friendlyName")] string? FriendlyName = null,
    [property: JsonPropertyName("changeEnumerationStatus")] string? ChangeEnumerationStatus = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public CloudEndpointDataSchema() : this(null, null, null, null, null, null, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a CloudEndpointDataSchema from a CloudEndpointResource.
    /// </summary>
    public static CloudEndpointDataSchema FromResource(CloudEndpointResource resource)
    {
        var data = resource.Data;
        return new CloudEndpointDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            data.StorageAccountResourceId?.ToString(),
            data.AzureFileShareName,
            data.StorageAccountTenantId.HasValue ? data.StorageAccountTenantId.ToString() : default,
            data.PartnershipId?.ToString(),
            data.ProvisioningState,
            data.LastOperationName,
            data.LastWorkflowId,
            data.FriendlyName,
            data.ChangeEnumerationStatus?.ToString()
        );
    }
}
