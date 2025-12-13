// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents file share-related limits in a specific subscription/location schema.
/// </summary>
public class FileShareLimitsSchema
{
    /// <summary>
    /// Gets or sets the maximum number of file shares that can be created.
    /// </summary>
    [JsonPropertyName("maxFileShares")]
    public int MaxFileShares { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of snapshots allowed per file share.
    /// </summary>
    [JsonPropertyName("maxFileShareSnapshots")]
    public int MaxFileShareSnapshots { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of subnets that can be associated with a file share.
    /// </summary>
    [JsonPropertyName("maxFileShareSubnets")]
    public int MaxFileShareSubnets { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of private endpoint connections allowed for a file share.
    /// </summary>
    [JsonPropertyName("maxFileSharePrivateEndpointConnections")]
    public int MaxFileSharePrivateEndpointConnections { get; set; }

    /// <summary>
    /// Gets or sets the minimum provisioned storage in GiB for a file share.
    /// </summary>
    [JsonPropertyName("minProvisionedStorageGiB")]
    public int MinProvisionedStorageGiB { get; set; }

    /// <summary>
    /// Gets or sets the maximum provisioned storage in GiB for a file share.
    /// </summary>
    [JsonPropertyName("maxProvisionedStorageGiB")]
    public int MaxProvisionedStorageGiB { get; set; }

    /// <summary>
    /// Gets or sets the minimum provisioned IOPS for a file share.
    /// </summary>
    [JsonPropertyName("minProvisionedIOPerSec")]
    public int MinProvisionedIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the maximum provisioned IOPS for a file share.
    /// </summary>
    [JsonPropertyName("maxProvisionedIOPerSec")]
    public int MaxProvisionedIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the minimum provisioned throughput in MiB/s for a file share.
    /// </summary>
    [JsonPropertyName("minProvisionedThroughputMiBPerSec")]
    public int MinProvisionedThroughputMiBPerSec { get; set; }

    /// <summary>
    /// Gets or sets the maximum provisioned throughput in MiB/s for a file share.
    /// </summary>
    [JsonPropertyName("maxProvisionedThroughputMiBPerSec")]
    public int MaxProvisionedThroughputMiBPerSec { get; set; }
}

/// <summary>
/// Constants used for calculating recommended values of file share provisioning properties schema.
/// </summary>
public class FileShareProvisioningConstantsSchema
{
    /// <summary>
    /// Gets or sets the base IO per second.
    /// </summary>
    [JsonPropertyName("baseIOPerSec")]
    public int BaseIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the scalar IO per second.
    /// </summary>
    [JsonPropertyName("scalarIOPerSec")]
    public double ScalarIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the base throughput in MiB per second.
    /// </summary>
    [JsonPropertyName("baseThroughputMiBPerSec")]
    public int BaseThroughputMiBPerSec { get; set; }

    /// <summary>
    /// Gets or sets the scalar throughput in MiB per second.
    /// </summary>
    [JsonPropertyName("scalarThroughputMiBPerSec")]
    public double ScalarThroughputMiBPerSec { get; set; }
}

/// <summary>
/// Represents file share limits output schema.
/// </summary>
public class FileShareLimitsOutputSchema
{
    /// <summary>
    /// Gets or sets the limits for the file share.
    /// </summary>
    public FileShareLimitsSchema? Limits { get; set; }

    /// <summary>
    /// Gets or sets the provisioning constants for the file share.
    /// </summary>
    [JsonPropertyName("provisioningConstants")]
    public FileShareProvisioningConstantsSchema? ProvisioningConstants { get; set; }
}

/// <summary>
/// Response structure for file share limits API schema.
/// </summary>
public class FileShareLimitsResponseSchema
{
    /// <summary>
    /// Gets or sets the properties of the file share limits.
    /// </summary>
    public FileShareLimitsOutputSchema? Properties { get; set; }
}
