// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Models;

/// <summary>
/// Represents file share provisioning parameters recommendation input schema.
/// </summary>
public class FileShareProvisioningRecommendationInputSchema
{
    /// <summary>
    /// Gets or sets the desired provisioned storage size of the share in GiB.
    /// </summary>
    [JsonPropertyName("provisionedStorageGiB")]
    public int ProvisionedStorageGiB { get; set; }
}

/// <summary>
/// Represents file share provisioning parameters recommendation output schema.
/// </summary>
public class FileShareProvisioningRecommendationOutputSchema
{
    /// <summary>
    /// Gets or sets the recommended value of provisioned IO per second of the share.
    /// </summary>
    [JsonPropertyName("provisionedIOPerSec")]
    public int ProvisionedIOPerSec { get; set; }

    /// <summary>
    /// Gets or sets the recommended value of provisioned throughput in MiB per second of the share.
    /// </summary>
    [JsonPropertyName("provisionedThroughputMiBPerSec")]
    public int ProvisionedThroughputMiBPerSec { get; set; }

    /// <summary>
    /// Gets or sets the available redundancy options for the share.
    /// </summary>
    [JsonPropertyName("availableRedundancyOptions")]
    public List<string>? AvailableRedundancyOptions { get; set; }
}

/// <summary>
/// Request structure for file share provisioning parameters recommendation API schema.
/// </summary>
public class FileShareProvisioningRecommendationRequestSchema
{
    /// <summary>
    /// Gets or sets the properties of the file share provisioning recommendation input.
    /// </summary>
    public FileShareProvisioningRecommendationInputSchema? Properties { get; set; }
}

/// <summary>
/// Response structure for file share provisioning parameters recommendation API schema.
/// </summary>
public class FileShareProvisioningRecommendationResponseSchema
{
    /// <summary>
    /// Gets or sets the properties of the file share provisioning recommendation output.
    /// </summary>
    public FileShareProvisioningRecommendationOutputSchema? Properties { get; set; }
}
