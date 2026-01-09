// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.ResourceManager.FileShares;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Data transfer object for File Share information.
/// </summary>
public sealed record FileShareDataSchema(
    [property: JsonPropertyName("id")] string? Id = null,
    [property: JsonPropertyName("name")] string? Name = null,
    [property: JsonPropertyName("type")] string? Type = null,
    [property: JsonPropertyName("location")] string? Location = null,
    [property: JsonPropertyName("tags")] Dictionary<string, string>? Tags = null,
    [property: JsonPropertyName("systemData")] SystemDataSchema? SystemData = null,
    [property: JsonPropertyName("properties")] FileSharePropertiesSchema? Properties = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public FileShareDataSchema() : this(null, null, null, null, null, null, null) { }

    /// <summary>
    /// Creates a FileShareDataSchema from a FileShareResource.
    /// </summary>
    public static FileShareDataSchema FromResource(FileShareResource resource)
    {
        var data = resource.Data;
        var props = data.Properties;

        return new FileShareDataSchema(
            data.Id.ToString(),
            data.Name,
            data.ResourceType.ToString(),
            data.Location.ToString(),
            new Dictionary<string, string>(data.Tags ?? new Dictionary<string, string>()),
            data.SystemData != null ? SystemDataSchema.FromSystemData(data.SystemData) : null,
            props != null ? new FileSharePropertiesSchema(
                props.MountName,
                props.HostName,
                props.MediaTier?.ToString(),
                props.Redundancy?.ToString(),
                props.Protocol?.ToString(),
                props.ProvisionedStorageInGiB,
                props.ProvisionedStorageNextAllowedDowngradeOn?.DateTime,
                props.ProvisionedIOPerSec,
                props.ProvisionedIOPerSecNextAllowedDowngradeOn?.DateTime,
                props.ProvisionedThroughputMiBPerSec,
                props.ProvisionedThroughputNextAllowedDowngradeOn?.DateTime,
                props.IncludedBurstIOPerSec,
                props.MaxBurstIOPerSecCredits,
                props.NfsProtocolRootSquash != null ? new NfsProtocolPropertiesSchema(props.NfsProtocolRootSquash?.ToString()) : null,
                props.PublicAccessAllowedSubnets?.Count > 0 ? new PublicAccessPropertiesSchema(props.PublicAccessAllowedSubnets.ToList()) : null,
                props.ProvisioningState?.ToString(),
                props.PublicNetworkAccess?.ToString(),
                props.PrivateEndpointConnections?.Select(pec => PrivateEndpointConnectionDataSchema.FromModel(pec)).ToList()
            ) : null
        );
    }
}

/// <summary>
/// Represents File Share properties schema.
/// </summary>
public sealed record FileSharePropertiesSchema(
    [property: JsonPropertyName("mountName")] string? MountName = null,
    [property: JsonPropertyName("hostName")] string? HostName = null,
    [property: JsonPropertyName("mediaTier")] string? MediaTier = null,
    [property: JsonPropertyName("redundancy")] string? Redundancy = null,
    [property: JsonPropertyName("protocol")] string? Protocol = null,
    [property: JsonPropertyName("provisionedStorageGiB")] int? ProvisionedStorageGiB = null,
    [property: JsonPropertyName("provisionedStorageNextAllowedDowngrade")] DateTime? ProvisionedStorageNextAllowedDowngrade = null,
    [property: JsonPropertyName("provisionedIOPerSec")] int? ProvisionedIOPerSec = null,
    [property: JsonPropertyName("provisionedIOPerSecNextAllowedDowngrade")] DateTime? ProvisionedIOPerSecNextAllowedDowngrade = null,
    [property: JsonPropertyName("provisionedThroughputMiBPerSec")] int? ProvisionedThroughputMiBPerSec = null,
    [property: JsonPropertyName("provisionedThroughputNextAllowedDowngrade")] DateTime? ProvisionedThroughputNextAllowedDowngrade = null,
    [property: JsonPropertyName("includedBurstIOPerSec")] int? IncludedBurstIOPerSec = null,
    [property: JsonPropertyName("maxBurstIOPerSecCredits")] long? MaxBurstIOPerSecCredits = null,
    [property: JsonPropertyName("nfsProtocolProperties")] NfsProtocolPropertiesSchema? NfsProtocolProperties = null,
    [property: JsonPropertyName("publicAccessProperties")] PublicAccessPropertiesSchema? PublicAccessProperties = null,
    [property: JsonPropertyName("provisioningState")] string? ProvisioningState = null,
    [property: JsonPropertyName("publicNetworkAccess")] string? PublicNetworkAccess = null,
    [property: JsonPropertyName("privateEndpointConnections")] List<PrivateEndpointConnectionDataSchema>? PrivateEndpointConnections = null)
{
    /// <summary>
    /// Default constructor for deserialization.
    /// </summary>
    public FileSharePropertiesSchema() : this(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null) { }
}

/// <summary>
/// Represents NFS protocol-specific properties schema.
/// </summary>
public sealed record NfsProtocolPropertiesSchema(
    [property: JsonPropertyName("rootSquash")] string? RootSquash = null);

/// <summary>
/// Represents public access properties schema for a file share.
/// </summary>
public sealed record PublicAccessPropertiesSchema(
    [property: JsonPropertyName("allowedSubnets")] List<string>? AllowedSubnets = null);
