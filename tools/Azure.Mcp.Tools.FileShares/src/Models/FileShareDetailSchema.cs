// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents a simplified view of File Share details schema for listing operations.
/// </summary>
public class FileShareDetailSchema
{
    /// <summary>
    /// Gets or sets the resource identifier.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the resource name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the resource location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the resource type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the resource tags.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the mount name of the file share.
    /// </summary>
    [JsonPropertyName("mountName")]
    public string? MountName { get; set; }

    /// <summary>
    /// Gets or sets the host name of the file share.
    /// </summary>
    [JsonPropertyName("hostName")]
    public string? HostName { get; set; }

    /// <summary>
    /// Gets or sets the media tier (SSD).
    /// </summary>
    [JsonPropertyName("mediaTier")]
    public string? MediaTier { get; set; }

    /// <summary>
    /// Gets or sets the redundancy level (Local, Zone).
    /// </summary>
    [JsonPropertyName("redundancy")]
    public string? Redundancy { get; set; }

    /// <summary>
    /// Gets or sets the file sharing protocol (NFS).
    /// </summary>
    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    /// <summary>
    /// Gets or sets the provisioned storage size in GiB.
    /// </summary>
    [JsonPropertyName("provisionedStorageGiB")]
    public int? ProvisionedStorageGiB { get; set; }

    /// <summary>
    /// Gets or sets the provisioned IO per second.
    /// </summary>
    [JsonPropertyName("provisionedIOPerSec")]
    public int? ProvisionedIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the provisioned throughput in MiB per second.
    /// </summary>
    [JsonPropertyName("provisionedThroughputMiBPerSec")]
    public int? ProvisionedThroughputMiBPerSec { get; set; }

    /// <summary>
    /// Gets or sets the provisioning state of the file share.
    /// </summary>
    [JsonPropertyName("provisioningState")]
    public string? ProvisioningState { get; set; }

    /// <summary>
    /// Gets or sets the public network access setting (Enabled, Disabled).
    /// </summary>
    [JsonPropertyName("publicNetworkAccess")]
    public string? PublicNetworkAccess { get; set; }

    /// <summary>
    /// Gets or sets the number of private endpoint connections.
    /// </summary>
    public int? PrivateEndpointConnectionCount { get; set; }
}
