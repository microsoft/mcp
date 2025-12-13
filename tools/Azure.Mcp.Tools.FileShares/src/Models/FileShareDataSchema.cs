// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents Azure File Share data schema.
/// </summary>
public class FileShareDataSchema
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
    /// Gets or sets the resource type.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the resource location.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets or sets the resource tags.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the system data (created/modified metadata).
    /// </summary>
    [JsonPropertyName("systemData")]
    public SystemDataSchema? SystemData { get; set; }

    /// <summary>
    /// Gets or sets the file share properties.
    /// </summary>
    public FileSharePropertiesSchema? Properties { get; set; }
}

/// <summary>
/// Represents File Share properties schema.
/// </summary>
public class FileSharePropertiesSchema
{
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
    /// Gets or sets the storage media tier (SSD).
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
    /// Gets or sets the date/time when provisioned storage can be reduced.
    /// </summary>
    [JsonPropertyName("provisionedStorageNextAllowedDowngrade")]
    public System.DateTime? ProvisionedStorageNextAllowedDowngrade { get; set; }

    /// <summary>
    /// Gets or sets the provisioned IO per second.
    /// </summary>
    [JsonPropertyName("provisionedIOPerSec")]
    public int? ProvisionedIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the date/time when provisioned IOPS can be reduced.
    /// </summary>
    [JsonPropertyName("provisionedIOPerSecNextAllowedDowngrade")]
    public System.DateTime? ProvisionedIOPerSecNextAllowedDowngrade { get; set; }

    /// <summary>
    /// Gets or sets the provisioned throughput in MiB per second.
    /// </summary>
    [JsonPropertyName("provisionedThroughputMiBPerSec")]
    public int? ProvisionedThroughputMiBPerSec { get; set; }

    /// <summary>
    /// Gets or sets the date/time when provisioned throughput can be reduced.
    /// </summary>
    [JsonPropertyName("provisionedThroughputNextAllowedDowngrade")]
    public System.DateTime? ProvisionedThroughputNextAllowedDowngrade { get; set; }

    /// <summary>
    /// Gets or sets the included burst IOPS.
    /// </summary>
    [JsonPropertyName("includedBurstIOPerSec")]
    public int? IncludedBurstIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the maximum burst IOPS credits.
    /// </summary>
    [JsonPropertyName("maxBurstIOPerSecCredits")]
    public long? MaxBurstIOPerSecCredits { get; set; }

    /// <summary>
    /// Gets or sets the NFS protocol-specific properties.
    /// </summary>
    [JsonPropertyName("nfsProtocolProperties")]
    public NfsProtocolPropertiesSchema? NfsProtocolProperties { get; set; }

    /// <summary>
    /// Gets or sets the public access properties.
    /// </summary>
    [JsonPropertyName("publicAccessProperties")]
    public PublicAccessPropertiesSchema? PublicAccessProperties { get; set; }

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
    /// Gets or sets the list of private endpoint connections.
    /// </summary>
    [JsonPropertyName("privateEndpointConnections")]
    public List<PrivateEndpointConnectionDataSchema>? PrivateEndpointConnections { get; set; }
}

/// <summary>
/// Represents NFS protocol-specific properties schema.
/// </summary>
public class NfsProtocolPropertiesSchema
{
    /// <summary>
    /// Gets or sets the root squash setting (NoRootSquash, RootSquash, AllSquash).
    /// </summary>
    [JsonPropertyName("rootSquash")]
    public string? RootSquash { get; set; }
}

/// <summary>
/// Represents public access properties schema for a file share.
/// </summary>
public class PublicAccessPropertiesSchema
{
    /// <summary>
    /// Gets or sets the list of allowed subnet resource IDs.
    /// </summary>
    [JsonPropertyName("allowedSubnets")]
    public List<string>? AllowedSubnets { get; set; }
}
