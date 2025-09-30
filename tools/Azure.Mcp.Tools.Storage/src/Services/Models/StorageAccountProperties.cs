// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Storage.Services.Models;

/// <summary>
/// A class representing the storage account properties.
/// </summary>
internal sealed class StorageAccountProperties
{
    /// <summary> Account HierarchicalNamespace enabled if sets to true. </summary>
    public bool? IsHnsEnabled { get; set; }
    /// <summary> Allow or disallow public access to all blobs or containers in the storage account. The default interpretation is false for this property. </summary>
    [JsonPropertyName("allowBlobPublicAccess")]
    public bool? AllowBlobPublicAccess { get; set; }
    /// <summary> Allows https traffic only to storage service if sets to true. </summary>
    [JsonPropertyName("supportsHttpsTrafficOnly")]
    public bool? EnableHttpsTrafficOnly { get; set; }
    /// <summary> Gets the status of the storage account at the time the operation was called. </summary>
    [JsonPropertyName("provisioningState")]
    public string? StorageAccountProvisioningState { get; set; }
    /// <summary> Gets the creation date and time of the storage account in UTC. </summary>
    [JsonPropertyName("creationTime")]
    public DateTimeOffset? CreatedOn { get; set; }
    /// <summary> Required for storage accounts where kind = BlobStorage. The access tier is used for billing. The 'Premium' access tier is the default value for premium block blobs storage account type and it cannot be changed for the premium block blobs storage account type. </summary>
    public string? AccessTier { get; set; }
}
